﻿using API.Cqrs.ContactService.Command;
using API.Entity;
using API.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace API.Cqrs.ContactService.Handler
{
    public class AddContactCommandHandler : IRequestHandler<AddContactCommand, ActionResult<Contact>>
    {
        public IContactRepository _contactRepository { get; set; }
        public IUnitOfWork _unitOfWork { get; set; }

        public AddContactCommandHandler(IContactRepository contactRepository, IUnitOfWork unitOfWork)
        {
            _contactRepository = contactRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ActionResult<Contact>> Handle(AddContactCommand command, CancellationToken cancellationToken)
        {
            var contactDto = command.newContactDto;

            if (contactDto.Phone.Any(c => !char.IsDigit(c) && c != '+')) return new BadRequestObjectResult("Phone can contains only digits.");
            
            if (!isEmailValid(contactDto.Email))
                return new BadRequestObjectResult("Incorrect email.");

            if (await _contactRepository.UniqueContactPhoneExists(contactDto.Name, contactDto.Phone, command.UserName)) return new NotFoundObjectResult("Contact with this phone already exists.");
            
            if (await _contactRepository.UniqueContactEmailExists(contactDto.Name, contactDto.Email, command.UserName)) return new NotFoundObjectResult("Contact with this email already exists.");

            var contact = _contactRepository.CreateContact(contactDto);

            // Check for changes in the context
            var changes = _unitOfWork.Context.ChangeTracker.Entries()
                                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                                .ToList();

            if (!changes.Any())
            {
                return new BadRequestObjectResult("No changes detected.");
            }

            await _unitOfWork.CompleteAsync();

            return new OkObjectResult(contact);
        }

        private bool isEmailValid(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
    }
}