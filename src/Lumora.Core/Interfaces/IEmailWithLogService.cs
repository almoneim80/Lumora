﻿namespace Lumora.Interfaces;

public interface IEmailWithLogService
{
    Task SendAsync(string subject, string fromEmail, string fromName, string[] recipients, string body, List<AttachmentDto>? attachments, int templateId = 0);

    Task SendToContactAsync(int contactId, string subject, string fromEmail, string fromName, string body, List<AttachmentDto>? attachments, int scheduleId = 0, int templateId = 0);
}
