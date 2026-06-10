using Monitoring.ViewModel;

namespace Monitoring.CommonFunction
{
    public interface ITokenExtraction
    {
        Task<TokenViewModel> ExtractToken();
    }
}
