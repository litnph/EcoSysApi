using MediatR;
using PFP.Application.Features.Comments.Common;

namespace PFP.Application.Features.Comments.GetComments;

public sealed record GetCommentsQuery(string ModuleCode, string EntityType, Guid EntityId) : IRequest<GetCommentsResponse>;
