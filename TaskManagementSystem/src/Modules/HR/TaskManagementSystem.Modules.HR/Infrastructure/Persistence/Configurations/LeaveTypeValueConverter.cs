using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManagementSystem.Modules.HR.Application;
using TaskManagementSystem.Modules.HR.Domain;

namespace TaskManagementSystem.Modules.HR.Infrastructure.Persistence.Configurations;

internal sealed class LeaveTypeValueConverter()
    : ValueConverter<LeaveType, string>(
        leaveType => leaveType.ToString(),
        value => LeaveTypeMapping.ParseFromStorage(value));
