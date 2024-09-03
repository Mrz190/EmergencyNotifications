using API.Dto;
using MediatR;

namespace API.Cqrs.ContactService.Query
{
    public class FindContactQuery : IRequest<IEnumerable<GetContactsDto>>
    {
        public string UserName { get; set; }
        public string ContactCreator { get; set; }
    }
}
