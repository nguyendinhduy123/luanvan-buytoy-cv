using buytoy.Models;
using buytoy.Models.Momo;

namespace buytoy.Services.Momo
{
    public interface IMomoService
    {

        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
