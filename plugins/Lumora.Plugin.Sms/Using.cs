global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;

global using Lumora.Plugin.Sms.Configuration;
global using Lumora.Plugin.Sms.Exceptions;
global using Lumora.Plugin.Sms.Interfaces;
global using Lumora.Plugin.Sms.DTOs;
global using Lumora.Plugin.Sms.Tasks;
global using Lumora.Plugin.Sms.Services;
global using Lumora.Application.Interfaces;
global using Lumora.Application.DTOs.Base;
global using Lumora.Domain.Exceptions;
global using Lumora.Domain.Entities.Tables;
global using Lumora.Domain.Enums;
global using Lumora.Infrastructure.Data;


global using Serilog;
global using PhoneNumbers;
global using Grpc.Core;
global using Twilio;
global using Twilio.Rest.Api.V2010.Account;

global using System.ComponentModel.DataAnnotations;
global using System.Security.Cryptography;


global using Lumora.Domain.Interfaces;
global using Lumora.Infrastructure.Interfaces;
global using Lumora.Application.Configuration;
global using Lumora.Application.Services;
global using Lumora.Domain.Entities;
global using Lumora.Infrastructure.BackgroundJobs;
global using Lumora.Infrastructure.Helpers;
global using Lumora.Infrastructure.Services.TaskSvc;
