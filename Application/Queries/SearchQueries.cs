using MediatR;
using TrialWorld.Core.Models;
using TrialWorld.Contracts;

namespace TrialWorld.Application.Queries;

public record GetSearchResultFeedbackQuery(string MediaId) : IRequest<TrialWorld.Contracts.SearchResultFeedbackDto>;
