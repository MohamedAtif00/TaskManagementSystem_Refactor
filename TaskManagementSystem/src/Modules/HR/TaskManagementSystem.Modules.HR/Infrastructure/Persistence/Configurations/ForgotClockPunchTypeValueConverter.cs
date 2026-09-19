using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class ForgotClockPunchTypeValueConverter()
    : ValueConverter<ForgotClockPunchType, string>(
        punchType => punchType.ToString(),
        value => ForgotClockPunchTypeMapping.ParseFromStorage(value));
