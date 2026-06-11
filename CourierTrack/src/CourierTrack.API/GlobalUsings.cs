global using System.Text;
global using System.Security.Claims;
global using System.Threading.RateLimiting;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.RateLimiting;
global using Microsoft.AspNetCore.SignalR;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Mvc;
global using FluentValidation;

global using CourierTrack.Domain.Common;
global using CourierTrack.Domain.Constants;
global using CourierTrack.Domain.Enums;

global using CourierTrack.Application;
global using CourierTrack.Application.DTOs;
global using CourierTrack.Application.Interfaces.Services;
global using CourierTrack.Application.Interfaces.Hubs;
global using CourierTrack.Application.Interfaces.Repositories;

global using CourierTrack.Infrastructure;

global using CourierTrack.Infrastructure.Data.Context;
global using CourierTrack.API.Middlewares;
global using CourierTrack.API.Hubs;


