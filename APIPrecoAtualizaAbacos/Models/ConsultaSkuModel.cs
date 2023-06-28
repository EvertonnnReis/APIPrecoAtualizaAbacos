namespace APIPrecoAtualizaAbacos.Models
{
    public class ConsultaSkuModel
    {
        public string PRODUTOCODIGOEXTERNO { get; set; }
        public DateTime DATAATUALIZACAOREGISTRO { get; set; }
        public DateTime DATACRIACAOREGISTRO { get; set; }
        public DateTime DATACONFIRMACAO { get; set; }
        public string APROVADORRESPONSAVELEMAIL { get; set; }
        public int APROVADO { get; set; }
        public int PRODUTOLISTAPRECOCODIGO { get; set; }
        public float PRODUTOPRECOTABELANOVO { get; set; }
        public float PRODUTOPRECOPROMOCIONALNOVO { get; set; }
    }
}
