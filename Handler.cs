using Amazon.Lambda.Core;
using System;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AwsDotnetCsharp
{
    public class Handler
    {
        public APIGatewayProxyResponse Calculator(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                dynamic requestPara = JsonConvert.DeserializeObject(request.Body);
                var result = CalculateRepayment(requestPara.loanAmount, requestPara.term, requestPara.rate, requestPara.fee);
                return new APIGatewayProxyResponse
                {
                    StatusCode = 200,
                    Body = requestPara
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    StatusCode = 500,
                    Body = ex.Message
                };
            }
        }
        public Repayment CalculateRepayment(double loanAmount, int term, double rate, double fee)
        {
            //calculate loan interest plus itself
            var loanRepayment = loanAmount * term * rate / 100 + loanAmount;
            // fee interest each month
            var feeInterest = fee * rate / 100;
            // calculate repayment for loan with the fee interst each month
            var repaymentForLoan = Math.Ceiling((loanRepayment / term + feeInterest) * 100) / 100;
            //for the fee only without its interest
            var repaymentForFee = Math.Round(fee / term, 2);

            var firstRepayment = Math.Round(repaymentForLoan + repaymentForFee, 2);
            var totalPayable = Math.Round(repaymentForLoan * term + fee, 2);
            var lastRepayment = Math.Round(totalPayable - firstRepayment * (term - 1), 2);

            return new Repayment(firstRepayment, lastRepayment, totalPayable);
        }
        public class requestBody
        {
            public double loanAmount { get; }
            public int term { get; }
            public double fee { get; }
            public double rate { get; }
        };
        public class Repayment
        {
            public Repayment(double regularRepayment, double lastRepayment, double totalPayable)
            {
                this.regularRepayment = regularRepayment;
                this.lastRepayment = lastRepayment;
                this.totalPayable = totalPayable;
            }
            public double regularRepayment { get; }
            public double lastRepayment { get; }
            public double totalPayable { get; }
        }
    }

}
