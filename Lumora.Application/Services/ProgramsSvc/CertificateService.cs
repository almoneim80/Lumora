using Lumora.Application.Interfaces.Infrastructure;
using Lumora.Application.Interfaces.PrograProgramIntfms;

namespace Lumora.Application.Services.Programs
{
    public class CertificateService(
            ICertificateRepository repository,
            IUnitOfWork unitOfWork,
            IPdfGenerator pdfGenerator,
            ILogger<CertificateService> logger,
            CertificateMessages messages) : ICertificateService
    {
        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateDetailsDto>> IssueCertificateAsync(int enrollmentId, CancellationToken cancellationToken)
        {
            try
            {
                if (enrollmentId <= 0)
                {
                    logger.LogWarning("CertificateService - IssueCertificateAsync : Invalid enrollmentId={EnrollmentId}", enrollmentId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var enrollment = await repository.GetEnrollmentWithDetailsAsync(enrollmentId, cancellationToken);
                if (enrollment == null)
                {
                    logger.LogWarning("CertificateService - IssueCertificateAsync : Enrollment not found. Id={EnrollmentId}", enrollmentId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                var isCompleted = await repository.IsProgramCompletedAsync(enrollment.UserId, enrollment.ProgramId, cancellationToken);
                if (!isCompleted)
                {
                    logger.LogWarning("CertificateService - IssueCertificateAsync : Program not completed. User={UserId}, Program={ProgramId}", enrollment.UserId, enrollment.ProgramId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgIncomplete, null, ErrorType.BadRequest);
                }

                var existingCertificate = await repository.GetCertificateByEnrollmentIdAsync(enrollmentId, cancellationToken);
                if (existingCertificate != null)
                {
                    logger.LogInformation("CertificateService - IssueCertificateAsync : Certificate already exists for enrollmentId={EnrollmentId}", enrollmentId);
                    var existingDto = BuildCertificateDto(enrollment, existingCertificate);
                    return new GeneralResult<ProgramCertificateDetailsDto>(true, messages.MsgCertificateAlreadyIssued, existingDto, ErrorType.Success);
                }

                var certificate = new ProgramCertificate
                {
                    EnrollmentId = enrollment.Id,
                    CertificateId = $"WEJHA-{Guid.NewGuid():N}".ToUpper(),
                    DeliveryMethod = DeliveryMethod.Online,
                    IssuedAt = DateTimeOffset.UtcNow,
                    ExpirationDate = enrollment.TrainingProgram.HasCertificateExpiration ? DateTimeOffset.UtcNow.AddMonths(enrollment.TrainingProgram.CertificateValidityInMonth) : null,
                    Status = CertificateStatus.Issued,
                    IssuedBy = "System"
                };

                repository.Add(certificate);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var dto = BuildCertificateDto(enrollment, certificate);
                logger.LogInformation("CertificateService - IssueCertificateAsync : Certificate issued successfully. Id={CertificateId}", certificate.Id);
                return new GeneralResult<ProgramCertificateDetailsDto>(true, messages.MsgCertificateIssued, dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - IssueCertificateAsync : Error issuing certificate.");
                return new GeneralResult<ProgramCertificateDetailsDto>(
                    false, messages.GetUnexpectedErrorMessage("issuing certificate."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<int>> CountProgramCertificatesAsync(int programId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق من المدخلات (Input Validation)
                if (programId <= 0)
                {
                    logger.LogWarning("CertificateService - CountProgramCertificatesAsync : Invalid programId={ProgramId}", programId);
                    return new GeneralResult<int>(false, messages.MsgIdInvalid, 0, ErrorType.BadRequest);
                }

                // 2. التحقق من وجود البرنامج عبر الـ Repository بدلاً من DbContext مباشر
                var programExists = await repository.ProgramExistsAsync(programId, cancellationToken);
                if (!programExists)
                {
                    logger.LogWarning("CertificateService - CountProgramCertificatesAsync : Program not found. Id={ProgramId}", programId);
                    return new GeneralResult<int>(false, messages.MsgDataNotFound, 0, ErrorType.NotFound);
                }

                // 3. جلب العدد باستخدام الدالة الجاهزة في المستودع
                var count = await repository.GetIssuedCertificatesCountAsync(programId, cancellationToken);

                if (count == 0)
                {
                    logger.LogInformation("CertificateService - CountProgramCertificatesAsync : No certificates found for programId={ProgramId}", programId);
                    return new GeneralResult<int>(false, messages.MsgNoCertificatesFound, 0, ErrorType.NotFound);
                }

                logger.LogInformation("CertificateService - CountProgramCertificatesAsync : Found {Count} certificates for programId={ProgramId}", count, programId);
                return new GeneralResult<int>(true, messages.MsgCertificateCountRetrieved, count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - CountProgramCertificatesAsync : Error counting certificates");
                return new GeneralResult<int>(false, messages.GetUnexpectedErrorMessage("Counting Certificates"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateDetailsDto>> GetByIdAsync(int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation (Business Logic)
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - GetByIdAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. Data Retrieval via Repository (Infrastructure Abstraction)
                var certificate = await repository.GetCertificateWithDetailsAsync(certificateId, cancellationToken);

                // 3. Null Check
                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - GetByIdAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 4. Mapping & Response
                var dto = BuildCertificateDto(certificate.ProgramEnrollment, certificate);

                logger.LogInformation("CertificateService - GetByIdAsync : Certificate retrieved. Id={CertificateId}", certificateId);
                return new GeneralResult<ProgramCertificateDetailsDto>(true, messages.MsgDataRetrieved, dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GetByIdAsync : Error retrieving certificate.");
                return new GeneralResult<ProgramCertificateDetailsDto>(
                    false,
                    messages.GetUnexpectedErrorMessage("retrieving certificate."),
                    null,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<ProgramCertificateListDto>>> GetUserCertificatesAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق الأولي من المدخلات
                if (string.IsNullOrWhiteSpace(userId))
                {
                    logger.LogWarning("CertificateService - GetUserCertificatesAsync : Invalid userId.");
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. التحقق من وجود المستخدم وصلاحيته عبر المستودع
                var user = await repository.GetUserForValidationAsync(userId, cancellationToken);

                if (user == null)
                {
                    logger.LogInformation("CertificateService - GetUserCertificatesAsync : User not found or inactive. Id={UserId}", userId);
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                // 3. جلب البيانات كـ Entities من المستودع
                var certificatesEntities = await repository.GetUserCertificatesAsync(userId, cancellationToken);

                if (certificatesEntities == null || !certificatesEntities.Any())
                {
                    logger.LogInformation("CertificateService - GetUserCertificatesAsync : No certificates found for user {UserId}", userId);
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgNoCertificatesFound, null, ErrorType.NotFound);
                }

                // 4. تحويل البيانات (Mapping) من Entity إلى DTO في طبقة التطبيق
                var certificatesDtos = certificatesEntities.Select(c => new ProgramCertificateListDto
                {
                    CertificateId = c.Id,
                    CertificateCode = c.CertificateId,
                    ProgramName = c.ProgramEnrollment?.TrainingProgram?.Name ?? string.Empty,
                    IssuedAt = c.IssuedAt,
                    ExpirationDate = c.ExpirationDate,
                    Status = c.Status,
                    DeliveryMethod = c.DeliveryMethod
                }).ToList();

                logger.LogInformation("CertificateService - GetUserCertificatesAsync : {Count} Certificates retrieved for user {UserId}", certificatesDtos.Count, userId);

                return new GeneralResult<List<ProgramCertificateListDto>>(
                    true, messages.MsgCertificateListRetrieved, certificatesDtos);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GetUserCertificatesAsync : Error retrieving certificates for user {UserId}", userId);
                return new GeneralResult<List<ProgramCertificateListDto>>(
                    false, messages.GetUnexpectedErrorMessage("retrieving user certificates"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RevokeCertificateAsync(int certificateId, string reason, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Validation Logic
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - RevokeCertificateAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                if (string.IsNullOrWhiteSpace(reason))
                {
                    logger.LogWarning("CertificateService - RevokeCertificateAsync : Reason is required.");
                    return new GeneralResult(false, messages.MsgRevocationReasonRequired, null, ErrorType.BadRequest);
                }

                // 2. Data Retrieval via Repository (No DbContext here)
                var certificate = await repository.GetByIdAsync(certificateId, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - RevokeCertificateAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 3. Business Rule Validation
                if (certificate.Status != CertificateStatus.Issued)
                {
                    logger.LogInformation("CertificateService - RevokeCertificateAsync : Certificate is not in issued status. Id={CertificateId}", certificateId);
                    return new GeneralResult(false, messages.MsgCertificateCannotBeRevoked, null, ErrorType.BadRequest);
                }

                // 4. State Change
                certificate.Status = CertificateStatus.Revoked;
                certificate.Notes = reason;
                certificate.UpdatedAt = DateTimeOffset.UtcNow;

                repository.Update(certificate);

                // 5. Atomic Persistence via UnitOfWork
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("CertificateService - RevokeCertificateAsync : Certificate revoked. Id={CertificateId}", certificateId);
                return new GeneralResult(true, messages.MsgCertificateRevoked, null);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - RevokeCertificateAsync : Error revoking certificate.");
                return new GeneralResult(false, messages.GetUnexpectedErrorMessage("revoking certificate"), null, ErrorType.InternalServerError);
            }
        }

        public async Task<GeneralResult<string>> GeneratePublicVerificationCodeAsync(int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق الأولي من المدخلات
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - GeneratePublicVerificationCodeAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. جلب الكيان عبر المستودع بدلاً من DbContext
                var certificate = await repository.GetByIdAsync(certificateId, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 3. التحقق من حالة الشهادة (Business Rule)
                if (certificate.Status != CertificateStatus.Issued)
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Cannot generate code for non-issued certificate. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgCertificateCannotGenerateCode, null, ErrorType.BadRequest);
                }

                // 4. التحقق إذا كان الكود موجوداً مسبقاً
                if (!string.IsNullOrWhiteSpace(certificate.VerificationCode))
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Code already exists. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(true, messages.MsgVerificationCodeRetrieved, certificate.VerificationCode);
                }

                // 5. تنفيذ منطق توليد الكود وتحديث الكيان
                var code = $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                certificate.VerificationCode = code;
                certificate.UpdatedAt = DateTimeOffset.UtcNow;

                // إبلاغ المستودع بالتحديث (اختياري حسب إعدادات EF Tracking ولكن يفضل للوضوح)
                repository.Update(certificate);

                // 6. الحفظ عبر UnitOfWork لضمان فصل المسؤوليات
                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Verification code generated. Id={CertificateId}", certificateId);
                return new GeneralResult<string>(true, messages.MsgVerificationCodeGenerated, code);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GeneratePublicVerificationCodeAsync : Error generating verification code.");
                return new GeneralResult<string>(
                    false,
                    messages.GetUnexpectedErrorMessage("generating verification code"),
                    null,
                    ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateFileDto>> ExportCertificatePdfAsync(int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق الأولي
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - ExportCertificatePdfAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(
                        false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                // 2. جلب البيانات عبر المستودع (بدلاً من DbContext)
                var certificate = await repository.GetCertificateWithDetailsAsync(certificateId, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - ExportCertificatePdfAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(
                        false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                // 3. التحقق من منطق الأعمال (Business Rules)
                if (certificate.Status != CertificateStatus.Issued)
                {
                    logger.LogInformation("CertificateService - ExportCertificatePdfAsync : Certificate is not issued. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(
                        false, messages.MsgCertificateNotIssued, null, ErrorType.BadRequest);
                }

                if (certificate.ExpirationDate.HasValue && certificate.ExpirationDate < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation("CertificateService - ExportCertificatePdfAsync : Certificate is expired. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(false, messages.MsgCertificateExpired, null, ErrorType.BadRequest);
                }

                // 4. توليد محتوى الـ HTML (منطق داخلي بالخدمة)
                var htmlContent = await GenerateCertificateHtmlAsync(certificate);

                // 5. توليد الـ PDF عبر الواجهة (تطهير من IronPDF)
                // ملاحظة: تم استخدام _pdfGenerator بدلاً من ChromePdfRenderer المباشر
                var pdfBytes = pdfGenerator.GenerateFromHtml(htmlContent);

                var fileDto = new ProgramCertificateFileDto
                {
                    FileName = $"Certificate_{certificate.CertificateId}.pdf",
                    ContentType = "application/pdf",
                    FileBytes = pdfBytes
                };

                logger.LogInformation("CertificateService - ExportCertificatePdfAsync : PDF generated successfully. Id={CertificateId}", certificateId);
                return new GeneralResult<ProgramCertificateFileDto>(
                    true, messages.MsgCertificatePdfGenerated, fileDto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - ExportCertificatePdfAsync : Error generating certificate PDF.");
                return new GeneralResult<ProgramCertificateFileDto>(
                    false, messages.GetUnexpectedErrorMessage("generating certificate PDF"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateDetailsDto>> VerifyCertificateAsync(string code, CancellationToken cancellationToken)
        {
            try
            {
                // 1. التحقق الأولي (Validation)
                if (string.IsNullOrWhiteSpace(code))
                {
                    logger.LogWarning("CertificateService - VerifyCertificateAsync : Invalid verification code");
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgVerificationCodeRequired, null, ErrorType.BadRequest);
                }

                // 2. استخدام المستودع بدلاً من DbContext (نقاء المعمارية)
                // الدالة GetByVerificationCodeAsync تحتوي بالفعل على الـ Includes اللازمة داخل الـ Repository
                var certificate = await repository.GetByVerificationCodeAsync(code, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - VerifyCertificateAsync : Certificate not found for code={Code}", code);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgVerificationCodeInvalid, null, ErrorType.NotFound);
                }

                // 3. تطبيق قواعد العمل (Business Rules)
                if (certificate.Status != CertificateStatus.Issued)
                {
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgCertificateNotIssued, null, ErrorType.BadRequest);
                }

                // 4. تحديث الحالة (State Mutation)
                if (certificate.VerifiedAt == null)
                {
                    certificate.VerifiedAt = DateTimeOffset.UtcNow;

                    // نستخدم المستودع لإبلاغ EF بأن الكيان قد تغير
                    repository.Update(certificate);

                    // نستخدم UnitOfWork لحفظ التغييرات (الحفظ هو مسؤولية الـ Application Service)
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("CertificateService - VerifyCertificateAsync : VerifiedAt updated for code={Code}", code);
                }

                // 5. بناء النتيجة
                var dto = BuildCertificateDto(certificate.ProgramEnrollment, certificate);
                return new GeneralResult<ProgramCertificateDetailsDto>(true, messages.MsgCertificateVerified, dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - VerifyCertificateAsync : Error verifying certificate.");
                return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.GetUnexpectedErrorMessage("verifying certificate"), null, ErrorType.InternalServerError);
            }
        }

        #region PRIVATE METHODS
        private ProgramCertificateDetailsDto BuildCertificateDto(ProgramEnrollment enrollment, ProgramCertificate certificate)
        {
            return new ProgramCertificateDetailsDto
            {
                Title = enrollment.TrainingProgram.Name ?? string.Empty,
                StudentName = enrollment.User?.FullName,
                StudentEmail = enrollment.User?.Email,
                StudentPhone = enrollment.User?.PhoneNumber,
                ProgramName = enrollment.TrainingProgram?.Name,
                IssuedAt = certificate.IssuedAt,
                CertificateId = certificate.CertificateId,
                ExpirationDate = certificate.ExpirationDate
            };
        }

        private async Task<string> GenerateCertificateHtmlAsync(ProgramCertificate certificate)
        {
            var enrollment = certificate.ProgramEnrollment;
            var user = enrollment.User;
            var program = enrollment.TrainingProgram;
            var issuedAt = certificate.IssuedAt?.UtcDateTime.ToString("MMMM dd, yyyy");

            var verificationUrl = $"https://yourdomain.com/certificate/verify?code={certificate.VerificationCode}";
            var qrImageUrl = $"https://chart.googleapis.com/chart?cht=qr&chs=150x150&chl={Uri.EscapeDataString(verificationUrl)}";

            var html = $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='utf-8'>
                    <style>
                        body {{ font-family: 'Segoe UI', sans-serif; background: #f7f9fc; margin: 0; }}
                        .certificate {{ width: 1000px; height: 700px; margin: auto; padding: 40px; background: white; border: 10px solid #003366; position: relative; }}
                        .title {{ text-align: center; font-size: 32px; font-weight: bold; color: #003366; margin-top: 40px; }}
                        .subtitle {{ text-align: center; font-size: 18px; margin-top: 20px; }}
                        .name {{ text-align: center; font-size: 28px; font-family: Cursive; color: #0088cc; margin: 30px 0; }}
                        .description {{ text-align: center; font-size: 16px; color: #333; width: 80%; margin: auto; }}
                        .footer {{ position: absolute; bottom: 50px; left: 0; width: 100%; text-align: center; }}
                        .signatures {{ display: flex; justify-content: space-around; margin-top: 40px; font-size: 14px; color: #666; }}
                        .seal {{ width: 60px; margin: 20px auto; }}
                        .qr-code {{
                            position: absolute;
                            bottom: 40px;
                            right: 40px;
                            text-align: center;
                            font-size: 10px;
                            color: #999;
                        }}
                        .qr-code img {{ width: 100px; height: 100px; }}
                    </style>
                </head>
                <body>
                    <div class='certificate'>
                        <div class='title'>Certificate of Completion</div>
                        <div class='subtitle'>This certificate is proudly awarded to</div>
                        <div class='name'>{user.FullName}</div>
                        <div class='subtitle'>for successfully completing the program</div>
                        <div class='name'>{program.Name}</div>
                        <div class='description'>Issued on {issuedAt} — Certificate ID: {certificate.CertificateId}</div>

                        <div class='footer'>
                            <img src='https://yourdomain.com/seal.png' class='seal' />
                            <div class='signatures'>
                                <div>Authorized Signature</div>
                                <div>Program Coordinator</div>
                            </div>
                        </div>

                        <div class='qr-code'>
                            <img src='{qrImageUrl}' alt='QR Code' />
                            <div class='qr-label'>Scan to verify</div>
                        </div>
                    </div>
                </body>
                </html>";

            return await Task.FromResult(html);
        }
        #endregion
    }
}
