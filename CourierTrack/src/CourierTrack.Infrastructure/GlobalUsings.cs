global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.EntityFrameworkCore;

global using CourierTrack.Domain.Entities;
global using CourierTrack.Infrastructure.Data.Context;
global using CourierTrack.Infrastructure.Repositories.Interfaces;
global using CourierTrack.Application.Interfaces;
global using CourierTrack.Application.Options;