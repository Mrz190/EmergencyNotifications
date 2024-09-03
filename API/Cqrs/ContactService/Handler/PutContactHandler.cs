using API.Cqrs.ContactService.Command;
using API.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Cqrs.ContactService.Handler
{
    public class PutContactHandler : IRequestHandler<PutContactCommand, IActionResult>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public PutContactHandler(IContactRepository contactRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _contactRepository = contactRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Handle(PutContactCommand command, CancellationToken cancellationToken)
        {
            var existingContact = await _contactRepository.GetContactByIdAsync(command.Id);
            if (existingContact == null) return new NotFoundResult();

            if (await _contactRepository.UniqueContactPhoneExists(command.Id, command.UpdatedContact.Phone, command.UserName))
                return new BadRequestObjectResult("A contact with such a phone already exists in your possession");
            if (await _contactRepository.UniqueContactEmailExists(command.Id, command.UpdatedContact.Email, command.UserName))
                return new BadRequestObjectResult("A contact with such an email already exists in your possession");
            if (await _contactRepository.IsValidEmail(command.UpdatedContact.Email) == false)
                return new BadRequestObjectResult("Email is incorrect!");

            _mapper.Map(command.UpdatedContact, existingContact);

            try
            {
                await _contactRepository.UpdateContactAsync(existingContact);

                var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                    .ToList();

                if (!changes.Any())
                {
                    return new BadRequestObjectResult("No changes detected.");
                }
                await _unitOfWork.CompleteAsync();

                return new OkObjectResult("Object was edited.");
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
}
