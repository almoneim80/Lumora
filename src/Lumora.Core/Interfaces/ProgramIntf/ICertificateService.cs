namespace Lumora.Interfaces.PrograProgramIntfms
{
    public interface ICertificateService
    {
        /// <summary>
        /// Issue a certificate for a specific program enrollment.
        /// </summary>
        Task<GeneralResult<ProgramCertificateDetailsDto>> IssueCertificateAsync(int enrollmentId, CancellationToken cancellationToken);

        /// <summary>
        /// Count the number of certificates issued for a specific program.
        /// </summary>
        Task<GeneralResult<int>> CountProgramCertificatesAsync(int programId, CancellationToken cancellationToken);

        /// <summary>
        /// Get certificate details by certificate ID.
        /// </summary>
        Task<GeneralResult<ProgramCertificateDetailsDto>> GetByIdAsync(int certificateId, CancellationToken cancellationToken);

        /// <summary>
        /// Get all certificates issued to a specific user.
        /// </summary>
        Task<GeneralResult<List<ProgramCertificateListDto>>> GetUserCertificatesAsync(string userId, CancellationToken cancellationToken);

        /// <summary>
        /// Revoke a certificate (administrative action).
        /// </summary>
        Task<GeneralResult> RevokeCertificateAsync(int certificateId, string reason, CancellationToken cancellationToken);

        /// <summary>
        /// Generate a public verification code for a certificate.
        /// </summary>
        Task<GeneralResult<string>> GeneratePublicVerificationCodeAsync(int certificateId, CancellationToken cancellationToken);

        /// <summary>
        /// Export certificate to PDF format (if applicable).
        /// </summary>
        Task<GeneralResult<ProgramCertificateFileDto>> ExportCertificatePdfAsync(int certificateId, CancellationToken cancellationToken);

        /// <summary>
        /// Verify certificate by public code.
        /// </summary>
        Task<GeneralResult<ProgramCertificateDetailsDto>> VerifyCertificateAsync(string code, CancellationToken cancellationToken);
    }
}
