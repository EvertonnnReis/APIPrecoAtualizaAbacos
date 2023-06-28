using APIPrecoAtualizaAbacos.Repositorios;

namespace APIPrecoAtualizaAbacos.Services
{
    public class Services: IServices
    {
        private readonly IRepositorio _repositorio;
        public Services(IRepositorio repositorio)
        {
            _repositorio = repositorio;
        }
    }
}
