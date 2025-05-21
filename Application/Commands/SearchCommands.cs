
using MediatR;

namespace TrialWorld.Application.Commands;

public record SubmitSearchFeedbackCommand(string MediaId, bool Relevant, string? Comments) : IRequest<bool>;
