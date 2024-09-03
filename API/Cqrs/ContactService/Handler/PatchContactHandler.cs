using API.Cqrs.ContactService.Command;
using API.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PatchContactHandler : IRequestHandler<PatchContactCommand, IActionResult>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public PatchContactHandler(IContactRepository contactRepository, IMapper mapper, IUnitOfWork unitOfWork)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Handle(PatchContactCommand command, CancellationToken cancellationToken)
    {
        var existingContact = await _contactRepository.GetContactByIdAsync(command.Id);
        if (existingContact == null)
            return new NotFoundResult();

        // Применяем изменения
        _mapper.Map(command.UpdatedContact, existingContact);

        try
        {
            await _contactRepository.UpdateContactAsync(existingContact);

            var changes = _unitOfWork.Context.ChangeTracker.Entries()
                            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                            .ToList();

            if (!changes.Any())
                return new BadRequestObjectResult("No changes detected.");

            await _unitOfWork.CompleteAsync();
            return new OkObjectResult(existingContact);
        }
        catch
        {
            return new ObjectResult("An error occurred while updating the contact.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
