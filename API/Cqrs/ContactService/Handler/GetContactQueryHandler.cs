using API.Cqrs.ContactService.Query;
using API.Dto;
using API.Interfaces;
using MediatR;

namespace API.Cqrs.ContactService.Handler
{
    public class GetContactQueryHandler : IRequestHandler<GetContactQuery, IEnumerable<GetContactsDto>>
    {
        private readonly IContactRepository _contactRepository;
        public GetContactQueryHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<IEnumerable<GetContactsDto>> Handle(GetContactQuery query, CancellationToken cancellationToken)
        {
            return await _contactRepository.GetMyContacts(query.UserName);
        }
    }
}
