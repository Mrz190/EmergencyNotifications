using API.Cqrs.ContactService.Query;
using API.Dto;
using API.Interfaces;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetContactByIdHandler : IRequestHandler<GetContactByIdQuery, GetContactsDto>
{
    private readonly IContactRepository _contactRepository;
    private readonly IMapper _mapper;

    public GetContactByIdHandler(IContactRepository contactRepository, IMapper mapper)
    {
        _contactRepository = contactRepository;
        _mapper = mapper;
    }

    public async Task<GetContactsDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _contactRepository.GetContactByIdAsync(request.Id);
        if (contact == null)
        {
            return null;
        }

        return _mapper.Map<GetContactsDto>(contact);
    }
}