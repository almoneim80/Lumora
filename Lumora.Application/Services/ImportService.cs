namespace Lumora.Application.Services
{
    public class ImportService<T, TI> : IImportService<T, TI>
        where TI : BaseEntityWithId, new()
        where T : BaseEntityWithId, new()
    {
        protected AdditionalImportChecker additionalImportChecker = new AdditionalImportChecker();
        private readonly ILogger<ImportService<T, TI>> _logger;
        private readonly IImportFileReader _fileReader;
        private readonly IMapper _mapper;
        private readonly IImportRepository _importRepository;
        private readonly IEntityMetadataProvider _metadataProvider;
        private string? _cachedAlternateKeyName;

        public ImportService(
            IImportRepository importRepository,
            IMapper mapper,
            IImportFileReader fileReader,
            IEntityMetadataProvider entityMetadataProvider,
            ILogger<ImportService<T, TI>> logger)
        {
            _importRepository = importRepository;
            _logger = logger;
            _mapper = mapper;
            _fileReader = fileReader;
            _metadataProvider = entityMetadataProvider;
        }

        // import data from list
        public async Task<ImportResult> ImportFromListAsync(List<TI> importRecords)
        {
            var result = new ImportResult();
            var newRecords = new List<T>();
            var updatedRecords = new List<T>();
            var duplicates = new Dictionary<TI, object>();

            // 1. تفعيل وضع الاستيراد عبر المستودع
            _importRepository.SetImportMode(true);

            // 2. بناء الخرائط 
            var typeIdentifiersMap = BuildTypeIdentifiersMap(importRecords);
            var relatedObjectsMap = BuildRelatedObjectsMap(typeIdentifiersMap, importRecords, newRecords, duplicates);
            var relatedTObjectsMap = relatedObjectsMap[typeof(T)];

            additionalImportChecker.SetData(importRecords);
            for (var i = 0; i < importRecords.Count; i++)
            {
                var importRecord = importRecords[i];

                if (!additionalImportChecker.Check(i, result))
                {
                    result.Skipped++;
                    result.AddMessage($"Row number {i} skipped due to additional import checker.");
                    continue;
                }

                if (duplicates.TryGetValue(importRecord, out var identifierValue))
                {
                    string message = $"Row number {i} has a duplicate identification value {identifierValue} and will be skipped.";
                    _logger.LogInformation(i, message);
                    result.AddError(i, message);
                    result.Skipped++;
                    result.AddMessage($"Item with identifier {identifierValue} skipped because it is a duplicate.");
                    continue;
                }

                BaseEntityWithId? dbRecord = null;
                foreach (var identifierProperty in relatedTObjectsMap.IdentifierPropertyNames)
                {
                    var identifierPropertyInfo = importRecord.GetType().GetProperty(identifierProperty)!;
                    var propertyValue = identifierPropertyInfo.GetValue(importRecord);

                    if (propertyValue != null && relatedTObjectsMap[identifierProperty].TryGetValue(propertyValue, out dbRecord))
                    {
                        _mapper.Map(importRecord, dbRecord);
                        updatedRecords.Add((T)dbRecord!);
                        result.Updated++;
                        result.AddMessage($"Item with Id {dbRecord!.Id} successfully updated.");
                        break;
                    }
                }

                if (dbRecord == null)
                {
                    dbRecord = _mapper.Map<T>(importRecord);
                    newRecords.Add((T)dbRecord);
                    result.AddMessage($"Item with temporary Id {dbRecord.Id} successfully added.");
                    result.Added++;
                }
            }

            if (newRecords.Any())
            {
                await _importRepository.AddRangeAsync(newRecords);
            }

            if (updatedRecords.Any())
            {
                foreach (var record in updatedRecords)
                {
                    _importRepository.Update(record);
                }
            }

            await _importRepository.SaveChangesAsync();

            return result;
        }

        // import data from file 
        public async Task<ImportResult> ImportFromFileAsync(IFileStream file)
        {
            try
            {
                // 1. استخراج الامتداد
                var fileExtension = Path.GetExtension(file.FileName) ?? string.Empty;

                // 2. استدعاء الدالة الموحدة التي قمنا ببنائها في الواجهة الجديدة
                // لاحظ أننا نمرر الـ Stream والامتداد والقارئ يتكفل بالباقي
                var importRecords = await _fileReader.ReadFileAsync<TI>(file.OpenReadStream(), fileExtension);

                // 3. تمرير القائمة الجاهزة لمعالج البيانات الرئيسي
                return await ImportFromListAsync(importRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while importing data from file: {FileName}", file.FileName);
                throw new IOException("Error occurred while importing data from file.", ex);
            }
        }

        // fix date kind if needed 
        private void FixDateKindIfNeeded(T record)
        {
            if (record is IHasCreatedAt createdAtRecord)
            {
                createdAtRecord.CreatedAt = createdAtRecord.CreatedAt == DateTimeOffset.MinValue
                    ? DateTimeOffset.UtcNow
                    : createdAtRecord.CreatedAt.ToUniversalTime();
            }

            if (record is IHasUpdatedAt updatedAtRecord)
            {
                if (updatedAtRecord.UpdatedAt.HasValue)
                {
                    updatedAtRecord.UpdatedAt = updatedAtRecord.UpdatedAt.Value.ToUniversalTime();
                }
                else
                {
                    updatedAtRecord.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        // build related objects map 
        private TypedRelatedObjectsMap BuildRelatedObjectsMap(TypeIdentifiers typeIdentifiersMap, List<TI> importRecords, List<T> newRecords, Dictionary<TI, object> duplicates)
        {
            var typedRelatedObjectsMap = new TypedRelatedObjectsMap();

            foreach (var type in typeIdentifiersMap.Keys)
            {
                var identifierValues = typeIdentifiersMap[type];

                var relatedObjectsMap = new RelatedObjectsMap
                {
                    IdentifierPropertyNames = identifierValues.IdentifierPropertyNames,
                    SurrogateKeyPropertyNames = identifierValues.SurrogateKeyPropertyNames,
                    SurrogateKeyPropertyAttributes = identifierValues.SurrogateKeyPropertyAttributes,
                };

                var mappedObjectsCash = new Dictionary<TI, object>();

                foreach (var propertyName in identifierValues.Keys)
                {
                    var existingRecordsProperty = type.GetProperty(propertyName)!;
                    var importRecordsProperty = typeof(TI).GetProperty(propertyName)!;
                    var propertyValues = identifierValues[propertyName];
                    var predicate = BuildPropertyValuesPredicate(type, propertyName, propertyValues);
                    var existingObjectsDict = _importRepository.GetDynamicQueryable(type)
                        .Where(predicate).ToDictionary(x => existingRecordsProperty.GetValue(x)!, x => x);

                    Dictionary<object, TI>? importRecordsDict = null;

                    if (type == typeof(T))
                    {
                        var uniqueGroups = importRecords
                                            .Select(x => new { Identifier = importRecordsProperty.GetValue(x), Record = x })
                                            .Where(x => x.Identifier != null && x.Identifier.ToString() != "0" && x.Identifier.ToString() != string.Empty)
                                            .GroupBy(x => x.Identifier!);

                        importRecordsDict = uniqueGroups.ToDictionary(g => g.Key, g => g.First().Record);

                        duplicates.AddRangeIfNotExists(uniqueGroups
                                            .Where(g => g.Count() > 1)
                                            .SelectMany(g => g.Skip(1))
                                            .ToDictionary(x => x.Record, x => x.Identifier!));
                    }

                    relatedObjectsMap[propertyName] = propertyValues
                           .Select(uid =>
                           {
                               existingObjectsDict.TryGetValue(uid, out var record);

                               if (type == typeof(T) && importRecordsDict!.TryGetValue(uid, out var importRecord))
                               {
                                   if (record == null && !mappedObjectsCash.TryGetValue(importRecord, out record))
                                   {
                                       record = _mapper.Map<T>(importRecord);
                                       FixDateKindIfNeeded((T)record);
                                       newRecords.Add((T)record);
                                   }

                                   mappedObjectsCash[importRecord] = record;
                               }

                               return new { Uid = uid, Record = record };
                           })
                           .ToDictionary(x => x.Uid, x => x.Record as BaseEntityWithId);
                }

                typedRelatedObjectsMap[type] = relatedObjectsMap;
            }

            return typedRelatedObjectsMap;
        }

        // build type identifiers map 
        private TypeIdentifiers BuildTypeIdentifiersMap(List<TI> importRecords)
        {
            var typeIdentifiersMap = new TypeIdentifiers { { typeof(T), new IdentifierValues() }, };

            var idValues = importRecords
                .Where(r => r.Id > 0)
                .Select(r => (object)r.Id)
                .Distinct()
                .ToList();

            if (idValues.Count > 0)
            {
                // التعديل هنا: استخدام الـ Repository بدلاً من المصنع والـ using
                var existingIds = _importRepository.GetQueryable<T>()
                                                    .Where(e => idValues.Contains(e.Id))
                                                    .Select(e => (object)e.Id)
                                                    .ToList();

                if (existingIds.Count > 0)
                {
                    typeIdentifiersMap[typeof(T)]["Id"] = existingIds;
                    typeIdentifiersMap[typeof(T)].IdentifierPropertyNames.Add("Id");
                }
            }

            var uniqueIndexPropertyName = FindAlternateKeyPropertyName();

            if (uniqueIndexPropertyName != null)
            {
                var property = typeof(TI).GetProperty(uniqueIndexPropertyName)!;

                var uniqueValues = importRecords
                                       .Where(r => property.GetValue(r) != null && property.GetValue(r)!.ToString() != string.Empty)
                                       .Select(r => property.GetValue(r))
                                       .Distinct()
                                       .ToList();

                if (uniqueValues.Count > 0)
                {
                    typeIdentifiersMap[typeof(T)][uniqueIndexPropertyName] = uniqueValues!;
                    typeIdentifiersMap[typeof(T)].IdentifierPropertyNames.Add(uniqueIndexPropertyName);
                }
            }

            var importProperties = typeof(TI).GetProperties();

            foreach (var property in importProperties)
            {
                if (property.GetCustomAttributes(typeof(SurrogateForeignKeyAttribute), true).FirstOrDefault() is not SurrogateForeignKeyAttribute surrogateForeignKeyAttribute)
                {
                    continue;
                }

                var type = surrogateForeignKeyAttribute.RelatedType;
                var identifierName = surrogateForeignKeyAttribute.RelatedTypeUniqeIndex;

                var identifierValues = importRecords
                                       .Where(r => property.GetValue(r) != null && property.GetValue(r)!.ToString() != string.Empty)
                                       .Select(r => property.GetValue(r))
                                       .Distinct()
                                       .ToList();

                if (identifierValues.Count == 0)
                {
                    continue;
                }

                if (!typeIdentifiersMap.ContainsKey(type))
                {
                    typeIdentifiersMap[type] = new IdentifierValues();
                }

                if (!typeIdentifiersMap[type].ContainsKey(identifierName))
                {
                    typeIdentifiersMap[type][identifierName] = new List<object>();
                }

                typeIdentifiersMap[type][identifierName].AddRange(identifierValues!);
                typeIdentifiersMap[type][identifierName] = typeIdentifiersMap[type][identifierName].Distinct().ToList();

                typeIdentifiersMap[typeof(T)].SurrogateKeyPropertyNames.Add(property.Name);
                typeIdentifiersMap[typeof(T)].SurrogateKeyPropertyAttributes.Add(surrogateForeignKeyAttribute);
            }

            _logger.LogInformation($"Identifier Map: {string.Join(", ", typeIdentifiersMap[typeof(T)].Keys)}");
            return typeIdentifiersMap;
        }

        // find unique index property name 
        private string FindAlternateKeyPropertyName()
        {
            if (_cachedAlternateKeyName != null) return _cachedAlternateKeyName;
            _cachedAlternateKeyName = _metadataProvider.GetAlternateKeyPropertyName<T>();

            return _cachedAlternateKeyName!;
        }

        // build property values predicate 
        private Func<object, bool> BuildPropertyValuesPredicate(Type targetType, string propertyName, List<object> propertyValues)
        {
            // Get the property info for the property name
            var propertyInfo = targetType.GetProperty(propertyName);

            // Create a parameter expression for the object type
            var objectParam = Expression.Parameter(typeof(object), "o");

            // Convert the object parameter to the target type
            var convertedParam = Expression.Convert(objectParam, targetType);

            // Create the property access expression for the property name
            var propertyAccess = Expression.Property(convertedParam, propertyInfo!);

            // Convert the property access expression to type object
            var convertedPropertyAccess = Expression.Convert(propertyAccess, typeof(object));

            // Create the constant expression for the property values
            var valuesConstant = Expression.Constant(propertyValues, typeof(List<object>));
            var containsMethod = typeof(List<object>).GetMethod("Contains", new[] { typeof(object) });
            var containsExpression = Expression.Call(valuesConstant, containsMethod!, convertedPropertyAccess);

            // Create the lambda expression for the predicate
            var lambdaExpression = Expression.Lambda<Func<object, bool>>(containsExpression, objectParam);

            return lambdaExpression.Compile();
        }

        // additional import checker 
        protected class AdditionalImportChecker
        {
            public virtual void SetData(List<TI> importRecords)
            {
            }

            public virtual bool Check(int index, ImportResult result)
            {
                return true;
            }
        }
    }
}
