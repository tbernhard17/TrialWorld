using TrialWorld.Core.Models.Search;
using TrialWorld.Core.Models.Validation;

namespace TrialWorld.Core.Interfaces
{
    public interface ISearchQueryValidator
    {
        ValidationResult Validate(SearchQuery query);
        bool IsValidSyntax(string queryText);
        bool AreValidFilters(SearchFilters filters);
    }
}