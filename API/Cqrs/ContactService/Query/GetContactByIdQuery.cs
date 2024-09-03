using MediatR;
using API.Dto;

namespace API.Cqrs.ContactService.Query
{

    public class GetContactByIdQuery : IRequest<GetContactsDto>
    {
        public int Id { get; set; }

        public GetContactByIdQuery(int id)
        {
            Id = id;
        }
    }
}
