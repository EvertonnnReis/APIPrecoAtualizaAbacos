using APIPrecoAtualizaAbacos.Repositorios;
using APIPrecoAtualizaAbacos.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;

namespace APIPrecoAtualizaAbacos.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class RegistroController: ControllerBase
    {
        private readonly IRepositorio _repositorio;
        private readonly IServices _services;
        public RegistroController(IRepositorio repositorio, IServices services)
        {
            _repositorio = repositorio;
            _services = services;
        }
        [HttpGet("/IdentificaSKU")]
        public async Task<dynamic> IdentificaRegistros()
        {
            return await _repositorio.IdentificaRegistros();
        }
        [HttpGet("/ConsultaSKU")]
        public async Task<dynamic> ConsultaSku()
        {
            return await _repositorio.ConsultaSku();
        }
        [HttpPost("/InsereSKU")]
        public async Task<dynamic> InsereSku()
        {
            return await _repositorio.InsereSku();
        }

    }
}
