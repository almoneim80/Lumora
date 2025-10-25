using Lumora.Interfaces.PrograProgramIntfms;
namespace Lumora.Services.Programs
{
    public class CertificateService(
        PgDbContext dbContext, ILogger<CertificateService> logger, CertificateMessages messages) : ICertificateService
    {
        private readonly PgDbContext _dbContext = dbContext;

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

                var enrollment = await _dbContext.ProgramEnrollments.Include(e => e.TrainingProgram).Include(e => e.User)
                    .FirstOrDefaultAsync(e => e.Id == enrollmentId && !e.IsDeleted, cancellationToken);
                if (enrollment == null)
                {
                    logger.LogWarning("CertificateService - IssueCertificateAsync : Enrollment not found. Id={EnrollmentId}", enrollmentId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                var isCompleted = await _dbContext.TraineeProgresses.AsNoTracking().AnyAsync(p => p.ProgramId == enrollment.ProgramId
                && p.UserId == enrollment.UserId && p.Level == ProgressLevel.Program && p.IsCompleted && !p.IsDeleted, cancellationToken);
                if (!isCompleted)
                {
                    logger.LogWarning("CertificateService - IssueCertificateAsync : Program not completed. User={UserId}, Program={ProgramId}", enrollment.UserId, enrollment.ProgramId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgIncomplete, null, ErrorType.BadRequest);
                }

                var existingCertificate = await _dbContext.ProgramCertificates.FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId && !c.IsDeleted, cancellationToken);
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

                _dbContext.ProgramCertificates.Add(certificate);
                await _dbContext.SaveChangesAsync(cancellationToken);

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
                if (programId <= 0)
                {
                    logger.LogWarning("CertificateService - CountProgramCertificatesAsync : Invalid programId={ProgramId}", programId);
                    return new GeneralResult<int>(false, messages.MsgIdInvalid, 0, ErrorType.BadRequest);
                }

                var program = await _dbContext.TrainingPrograms.FirstOrDefaultAsync(p => p.Id == programId && !p.IsDeleted, cancellationToken);
                if (program == null)
                {
                    logger.LogWarning("CertificateService - CountProgramCertificatesAsync : Program not found. Id={ProgramId}", programId);
                    return new GeneralResult<int>(false, messages.MsgDataNotFound, 0, ErrorType.NotFound);
                }

                var count = await _dbContext.ProgramCertificates.AsNoTracking()
                    .CountAsync(c => c.ProgramEnrollment.ProgramId == programId && c.Status == CertificateStatus.Issued && !c.IsDeleted, cancellationToken);
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
                logger.LogError(ex, "TrainingProgramService - CountProgramCertificateAsync : Error counting certificates");
                return new GeneralResult<int>(false, messages.GetUnexpectedErrorMessage("Counting Certificates"), 0, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateDetailsDto>> GetByIdAsync(int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - GetByIdAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var certificate = await _dbContext.ProgramCertificates
                    .Include(c => c.ProgramEnrollment).ThenInclude(e => e.TrainingProgram)
                    .Include(c => c.ProgramEnrollment).ThenInclude(e => e.User)
                    .AsNoTracking().FirstOrDefaultAsync(c => c.Id == certificateId && !c.IsDeleted, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - GetByIdAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                var dto = BuildCertificateDto(certificate.ProgramEnrollment, certificate);
                logger.LogInformation("CertificateService - GetByIdAsync : Certificate retrieved. Id={CertificateId}", certificateId);
                return new GeneralResult<ProgramCertificateDetailsDto>(true, messages.MsgDataRetrieved, dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GetByIdAsync : Error retrieving certificate.");
                return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.GetUnexpectedErrorMessage("retrieving certificate."), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<List<ProgramCertificateListDto>>> GetUserCertificatesAsync(string userId, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    logger.LogWarning("CertificateService - GetUserCertificatesAsync : Invalid userId.");
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var user = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted && u.IsActive, cancellationToken);

                if (user == null)
                {
                    logger.LogInformation("CertificateService - GetUserCertificatesAsync : User not found. Id={UserId}", userId);
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgUserNotFound, null, ErrorType.NotFound);
                }

                var certificates = await _dbContext.ProgramCertificates.AsNoTracking()
                    .Include(c => c.ProgramEnrollment)
                        .ThenInclude(e => e.TrainingProgram)
                    .Where(c => c.ProgramEnrollment.UserId == userId &&
                                c.Status == CertificateStatus.Issued &&
                                !c.IsDeleted)
                    .OrderByDescending(c => c.IssuedAt)
                    .Select(c => new ProgramCertificateListDto
                    {
                        CertificateId = c.Id,
                        CertificateCode = c.CertificateId,
                        ProgramName = c.ProgramEnrollment.TrainingProgram.Name ?? string.Empty,
                        IssuedAt = c.IssuedAt,
                        ExpirationDate = c.ExpirationDate,
                        Status = c.Status,
                        DeliveryMethod = c.DeliveryMethod
                    })
                    .ToListAsync(cancellationToken);

                if (!certificates.Any())
                {
                    logger.LogInformation("CertificateService - GetUserCertificatesAsync : No certificates found for user {UserId}", userId);
                    return new GeneralResult<List<ProgramCertificateListDto>>(
                        false, messages.MsgNoCertificatesFound, null, ErrorType.NotFound);
                }

                logger.LogInformation("CertificateService - GetUserCertificatesAsync : Certificates retrieved for user {UserId}", userId);
                return new GeneralResult<List<ProgramCertificateListDto>>(true, messages.MsgCertificateListRetrieved, certificates);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GetUserCertificatesAsync : Error retrieving certificates.");
                return new GeneralResult<List<ProgramCertificateListDto>>(
                    false, messages.GetUnexpectedErrorMessage("retrieving user certificates"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult> RevokeCertificateAsync(int certificateId, string reason, CancellationToken cancellationToken)
        {
            try
            {
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

                var certificate = await _dbContext.ProgramCertificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && !c.IsDeleted, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - RevokeCertificateAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                if (certificate.Status != CertificateStatus.Issued)
                {
                    logger.LogInformation("CertificateService - RevokeCertificateAsync : Certificate is not in issued status. Id={CertificateId}", certificateId);
                    return new GeneralResult(false, messages.MsgCertificateCannotBeRevoked, null, ErrorType.BadRequest);
                }

                certificate.Status = CertificateStatus.Revoked;
                certificate.Notes = reason;
                certificate.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

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
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - GeneratePublicVerificationCodeAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var certificate = await _dbContext.ProgramCertificates
                    .FirstOrDefaultAsync(c => c.Id == certificateId && !c.IsDeleted, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

                if (certificate.Status != CertificateStatus.Issued)
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Cannot generate code for non-issued certificate. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(false, messages.MsgCertificateCannotGenerateCode, null, ErrorType.BadRequest);
                }

                if (!string.IsNullOrWhiteSpace(certificate.VerificationCode))
                {
                    logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Code already exists. Id={CertificateId}", certificateId);
                    return new GeneralResult<string>(true, messages.MsgVerificationCodeRetrieved, certificate.VerificationCode);
                }

                // Generate a new code
                var code = $"CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                certificate.VerificationCode = code;
                certificate.UpdatedAt = DateTimeOffset.UtcNow;

                await _dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("CertificateService - GeneratePublicVerificationCodeAsync : Verification code generated. Id={CertificateId}", certificateId);
                return new GeneralResult<string>(true, messages.MsgVerificationCodeGenerated, code);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CertificateService - GeneratePublicVerificationCodeAsync : Error generating verification code.");
                return new GeneralResult<string>(
                    false, messages.GetUnexpectedErrorMessage("generating verification code"), null, ErrorType.InternalServerError);
            }
        }

        /// <inheritdoc/>
        public async Task<GeneralResult<ProgramCertificateFileDto>> ExportCertificatePdfAsync(int certificateId, CancellationToken cancellationToken)
        {
            try
            {
                if (certificateId <= 0)
                {
                    logger.LogWarning("CertificateService - ExportCertificatePdfAsync : Invalid certificateId={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(
                        false, messages.MsgIdInvalid, null, ErrorType.BadRequest);
                }

                var certificate = await _dbContext.ProgramCertificates
                    .Include(c => c.ProgramEnrollment)
                        .ThenInclude(e => e.TrainingProgram)
                    .Include(c => c.ProgramEnrollment)
                        .ThenInclude(e => e.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == certificateId && !c.IsDeleted, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - ExportCertificatePdfAsync : Certificate not found. Id={CertificateId}", certificateId);
                    return new GeneralResult<ProgramCertificateFileDto>(
                        false, messages.MsgDataNotFound, null, ErrorType.NotFound);
                }

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

                // توليد HTML باستخدام Razor أو قالب HTML
                var htmlContent = await GenerateCertificateHtmlAsync(certificate);

                // تحويل HTML إلى PDF باستخدام IronPDF
                var renderer = new ChromePdfRenderer();
                var pdfDocument = renderer.RenderHtmlAsPdf(htmlContent);
                var pdfBytes = pdfDocument.BinaryData;

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
                if (string.IsNullOrWhiteSpace(code))
                {
                    logger.LogWarning("CertificateService - VerifyCertificateAsync : Invalid verification code");
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgVerificationCodeRequired, null, ErrorType.BadRequest);
                }

                var certificate = await _dbContext.ProgramCertificates
                    .Include(c => c.ProgramEnrollment).ThenInclude(e => e.TrainingProgram)
                    .Include(c => c.ProgramEnrollment).ThenInclude(e => e.User)
                    .FirstOrDefaultAsync(c => c.VerificationCode == code && !c.IsDeleted, cancellationToken);

                if (certificate == null)
                {
                    logger.LogInformation("CertificateService - VerifyCertificateAsync : Certificate not found for code={Code}", code);
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgVerificationCodeInvalid, null, ErrorType.NotFound);
                }

                if (certificate.Status != CertificateStatus.Issued)
                {
                    return new GeneralResult<ProgramCertificateDetailsDto>(false, messages.MsgCertificateNotIssued, null, ErrorType.BadRequest);
                }

                // Mark the certificate as verified if it's not already
                if (certificate.VerifiedAt == null)
                {
                    certificate.VerifiedAt = DateTimeOffset.UtcNow;
                    certificate.UpdatedAt = DateTimeOffset.UtcNow;
                    await _dbContext.SaveChangesAsync(cancellationToken);

                    logger.LogInformation("CertificateService - VerifyCertificateAsync : VerifiedAt updated for code={Code}", code);
                }

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
