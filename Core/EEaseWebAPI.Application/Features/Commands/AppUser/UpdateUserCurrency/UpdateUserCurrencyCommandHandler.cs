using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCurrency
{
    public class UpdateUserCurrencyCommandHandler : IRequestHandler<UpdateUserCurrencyCommandRequest, UpdateUserCurrencyCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public UpdateUserCurrencyCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<UpdateUserCurrencyCommandResponse> Handle(UpdateUserCurrencyCommandRequest request, CancellationToken cancellationToken)
        {
            await _userService.UpdateUserCurrency(request.Username, request.CurrencyCode);

            return new UpdateUserCurrencyCommandResponse
            {
                Response = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserCurrencyUpdatedSuccessfully),
                    Body = new()
                    {
                        Message = "Currency updated successfully"
                    }
                }
            };
        }
    }
}
