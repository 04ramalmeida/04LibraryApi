﻿using _04LibraryApi.Data;
using MailKit.Net.Smtp;
using MimeKit;

namespace _04LibraryApi.Helpers;

public class MailHelper : IMailHelper
{
    private readonly IConfiguration _configuration;

    public MailHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public MailResponse SendEmail(string to, string subject, string body)
    {
        var nameFrom = _configuration["Mail:NameFrom"];
        var from  = _configuration["Mail:From"];
        var smtp = _configuration["Mail:Smtp"];
        var port = _configuration["Mail:Port"];
        var password = _configuration["Mail:Password"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(nameFrom, from));
        message.To.Add(new MailboxAddress(to, to));
        message.Subject = subject;
        
        var builder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = builder.ToMessageBody();

        try
        {
            using (var client = new SmtpClient())
            {
                client.Connect(smtp, int.Parse(port), false);
                client.Authenticate(from, password);
                client.Send(message);
                client.Disconnect(true);
            }
        }
        catch (Exception e)
        {
            return new MailResponse
            {
                IsSuccess = false,
                Message = e.ToString()
            };
        }

        return new MailResponse
        {
            IsSuccess = true
        };
    }
}