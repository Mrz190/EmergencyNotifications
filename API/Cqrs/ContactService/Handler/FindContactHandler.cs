using API.Cqrs.ContactService.Query;
using API.Dto;
using API.Interfaces;
using MediatR;

namespace API.Cqrs.ContactService.Handler
{
    public class FindContactHandler : IRequestHandler<FindContactQuery, IEnumerable<GetContactsDto>>
    {
        private readonly IContactRepository _contactRepository;

        public FindContactHandler(IContactRepository contactRepository)
        {
            _contactRepository = contactRepository;
        }

        public async Task<IEnumerable<GetContactsDto>> Handle(FindContactQuery query, CancellationToken cancellationToken)
        {
            return await _contactRepository.GetContactByName(query.UserName, query.ContactCreator);
        }
    }
}
