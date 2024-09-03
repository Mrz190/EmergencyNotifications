using API.Cqrs.ContactService.Command;
using API.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Cqrs.ContactService.Handler
{
    public class DelContactHandler : IRequestHandler<DelContactCommand, IActionResult>
    {
        private readonly IContactRepository _contactRepository;
        private readonly IUnitOfWork _unitOfWork;
        public DelContactHandler(IUnitOfWork unitOfWork, IContactRepository contactRepository)
        {
            _unitOfWork = unitOfWork;
            _contactRepository = contactRepository;
        }

        public async Task<IActionResult> Handle(DelContactCommand command, CancellationToken cancellationToken)
        {
            var contactCreator = command.UserName;

            if (await _contactRepository.DeleteContactAsync(command.Id, contactCreator) == false) return new BadRequestResult();

            var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                    .ToList();

            if (!changes.Any())
            {
                return new BadRequestObjectResult("No changes detected.");
            }
            await _unitOfWork.CompleteAsync();

            return new OkObjectResult("Contact deleted.");
        }
    }
}
