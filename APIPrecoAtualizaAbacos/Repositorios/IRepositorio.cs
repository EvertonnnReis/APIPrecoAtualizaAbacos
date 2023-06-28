namespace APIPrecoAtualizaAbacos.Repositorios
{
    public interface IRepositorio
    {
        Task<dynamic> IdentificaRegistros();
        Task<dynamic> ConsultaSku();
        Task<dynamic> InsereSku();
    }
}
