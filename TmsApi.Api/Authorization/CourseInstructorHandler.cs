using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using TmsApi.Domain.Entities;

namespace TmsApi.Api.Authorization;

public class CourseInstructorHandler : AuthorizationHandler<CourseInstructorRequirement, Course>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CourseInstructorRequirement requirement, Course resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isInstructor = context.User.IsInRole("Instructor");
        var isAdmin = context.User.IsInRole("Admin");

        if (isAdmin)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        if (isInstructor && resource.InstructorId == userId)
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}