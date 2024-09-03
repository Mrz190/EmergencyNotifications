using API.Dto;
using MediatR;

namespace API.Cqrs.ContactService.Query
{
    public class GetContactQuery : IRequest<IEnumerable<GetContactsDto>>
    {
        public string UserName { get; set; }
    }
}
