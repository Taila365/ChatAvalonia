using Chat.Core.Models;
using Mapster;
using System;
using System.Collections.Generic;
using System.Text;

namespace Chat.Application.Dtos
{
    public class Mapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.ForType<Student, StudentDto>()
                .Map(dest => dest.Gender, src => src.Gender.ToString())
                .Map(dest => dest.Age, src => DateTime.Now.Year-src.Birthday.Year);
        }
    }

}
