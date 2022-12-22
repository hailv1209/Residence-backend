using System.Reflection;
using FluentEmail.Core;
using Residence.DTOs;

namespace Residence.Services;

public class EmailService<T>
{
    private readonly IFluentEmail _mail;
    public EmailService(IFluentEmail mail)
    {
        _mail = mail;
    }

    public async Task<bool> SendEmail(EmailDto<T> emailDto)
    {
        _mail.To(emailDto.To)
                     .Subject(emailDto.Subject)
                     .UsingTemplate(emailDto.Template, emailDto.Model);

        var result = await _mail.SendAsync();
        return result.Successful;
    }

    public async Task<bool> SendEmailWithEmbeddedTemplate(EmailDto<T> emailDto)
    {
        _mail.To(emailDto.To)
            .Subject(emailDto.Subject)
            .UsingTemplateFromEmbedded(emailDto.Template, emailDto.Model, this.GetType().GetTypeInfo().Assembly);

        var result = await _mail.SendAsync();
        return result.Successful;
    }
}