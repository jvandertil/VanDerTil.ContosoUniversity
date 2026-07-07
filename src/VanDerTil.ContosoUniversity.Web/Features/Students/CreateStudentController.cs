using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using VanDerTil.ContosoUniversity.Web.Infrastructure.Filters;
using VanDerTil.ContosoUniversity.Web.Infrastructure.Requests;

namespace VanDerTil.ContosoUniversity.Web.Features.Students;

[Route("/students/create")]
public class CreateStudentController : Controller
{
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(CreateStudentCommand command)
    {
        // Here you would typically handle the command, e.g., save the student to the database.
        // For this example, we'll just redirect to a success page.
        return RedirectToUrlJsonResult.To(Url.Action("Index", "ListStudents"));
    }

    public sealed class CreateStudentCommand : IRequest<int>
    {
        public string? FirstMidName { get; init; }

        public string? LastName { get; init; }

        public DateTime? EnrollmentDate { get; init; }
    }

    public sealed class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
    {
        public CreateStudentCommandValidator()
        {
            RuleFor(x => x.FirstMidName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.EnrollmentDate)
                .NotNull().WithMessage("Enrollment date is required.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Enrollment date cannot be in the future.");
        }
    }

    public sealed class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, int>
    {
        public Task<int> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
        {
            // Handle the command, e.g., save the student to the database.
            return Task.FromResult(-1);
        }
    }
}

