using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Exceptions.CreateUser;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using EEaseWebAPI.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommandRequest, CreateUserCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;


        public CreateUserCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<CreateUserCommandResponse> Handle(CreateUserCommandRequest request, CancellationToken cancellationToken)
        {
            CreateUserResponse result = await _userService.CreateAsync(new() 
            {
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                Password = request.Password,
                PasswordConfirm = request.PasswordConfirm,
                Gender = request.Gender,
                Username = request.Username,
                BornDate = request.BornDate
            });

            return new CreateUserCommandResponse
            {
                response = new()
                {
                    Body = result?.response?.Body,
                    Header = _headerService.HeaderCreate((int)StatusEnum.SuccessfullyCreated)

                }
            };
            
        }

        
    }

    
}
