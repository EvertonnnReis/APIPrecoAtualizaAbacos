using APIPrecoAtualizaAbacos.Models;
using Microsoft.Win32;
using System.Data;
using System.Data.SqlClient;

namespace APIPrecoAtualizaAbacos.Repositorios
{
    public class Repositorio : IRepositorio
    {
        private readonly string _connectrionString;
        private List<string> listaSkuFormatada;
        List<ConsultaSkuModel> registros = new List<ConsultaSkuModel>();
        public Repositorio(IConfiguration configuration)
        {
            _connectrionString = configuration.GetConnectionString("AbacosServer");
        }


        public async Task<dynamic> IdentificaRegistros()
        {
            List<IdentificaModel> registros = new List<IdentificaModel>();
            List<string> listaSku = new List<string>();

            // Conexão com o banco de dados
            using (var conn = new SqlConnection(_connectrionString))
            {
                await conn.OpenAsync();

                // Cria SQL de identificação
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                    SELECT B.PROS_EXT_COD AS SKU
                    FROM ABACOS.DBO.TCOM_PROLIS A (NOLOCK)
	                    INNER JOIN TCOM_PROSER B (NOLOCK) ON (A.PROS_COD = B.PROS_COD)
                    WHERE B.PROS_EXT_COD COLLATE Latin1_General_CI_AS NOT IN 
	                    (SELECT ProdutoCodigoExterno COLLATE Latin1_General_CI_AS AS SKU
	                    FROM [CONNECTPARTS].[DBO].[Precificacoes])
                    AND PROL_DAT_ALT >= '2023-06-23'
                    ORDER BY SKU ASC";


                    // Executa o SQL e obtém os resultados
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var registro = new IdentificaModel
                            {
                                SKU = reader.GetString(reader.GetOrdinal("SKU"))
                            };
                            // Se precisar trocar a virgula, usar o replace
                            registros.Add(registro);
                            listaSku.Add(registro.SKU);
                        }
                    }
                }
            }
            listaSkuFormatada = listaSku.Select(sku => "'" + sku + "'").ToList();
            return new { Lista = registros, ListaFormatada = listaSkuFormatada };
        }
        public async Task<dynamic> ConsultaSku()
        {
            var identificaRegistrosResultados = await IdentificaRegistros();
            string listaSkuFormatadaStr = string.Join(",", listaSkuFormatada.Select(sku => sku.Trim('\"')));


            // Conexão com o banco de dados
            using (var conn = new SqlConnection(_connectrionString))
            {
                await conn.OpenAsync();

                // Cria SQL de identificação
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"
                    SELECT  
                        B.PROS_EXT_COD AS PRODUTOCODIGOEXTERNO,
                        PROL_DAT_ALT AS DATAATUALIZACAOREGISTRO,
                        PROL_DAT_CAD AS DATACRIACAOREGISTRO,
                        GETDATE () AS DATACONFIRMACAO,
                        APROVADORRESPONSAVELEMAIL = 'OPERAÇÃO REALIZADA VIA ABACOS',
                        APROVADO = 1,
                        LISP_COD AS PRODUTOLISTAPRECOCODIGO,
                        PROL_VAL_PRETMP AS PRODUTOPRECOTABELANOVO,
                        PROL_VAL_PREPROTMP AS PRODUTOPRECOPROMOCIONALNOVO
                    FROM ABACOS.DBO.TCOM_PROLIS A (NOLOCK)
	                    INNER JOIN TCOM_PROSER B (NOLOCK) ON (A.PROS_COD = B.PROS_COD)
                    WHERE PROL_DAT_ALT IS NOT NULL AND PROL_VAL_PRETMP IS NOT NULL 
					AND PROL_VAL_PREPROTMP IS NOT NULL AND B.PROS_EXT_COD IN (" + listaSkuFormatadaStr + ")";


                    //B.PROS_EXT_COD IN (" + listaSkuFormatadaStr + ")";

                    // Executa o SQL e obtém os resultados
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var registro = new ConsultaSkuModel
                            {
                                PRODUTOCODIGOEXTERNO = reader.GetString(reader.GetOrdinal("PRODUTOCODIGOEXTERNO")),
                                DATAATUALIZACAOREGISTRO = reader.GetDateTime(reader.GetOrdinal("DATAATUALIZACAOREGISTRO")),
                                DATACRIACAOREGISTRO = reader.GetDateTime(reader.GetOrdinal("DATACRIACAOREGISTRO")),
                                DATACONFIRMACAO = reader.GetDateTime(reader.GetOrdinal("DATACONFIRMACAO")),
                                APROVADORRESPONSAVELEMAIL = "OPERAÇÃO REALIZADA VIA ABACOS",
                                APROVADO = 1,
                                PRODUTOLISTAPRECOCODIGO = reader.GetInt32(reader.GetOrdinal("PRODUTOLISTAPRECOCODIGO")),
                                PRODUTOPRECOTABELANOVO = Convert.ToSingle(reader.GetDouble(reader.GetOrdinal("PRODUTOPRECOTABELANOVO"))),
                                PRODUTOPRECOPROMOCIONALNOVO = Convert.ToSingle(reader.GetDouble(reader.GetOrdinal("PRODUTOPRECOPROMOCIONALNOVO")))
                            };
                            // Se precisar trocar a virgula, usar o replace]
                            registros.Add(registro);

                        }
                    }
                }
            }
            return registros;
        }
        public async Task<dynamic> InsereSku()
        {
            var SkuConsultados = await ConsultaSku();
            using (var conn = new SqlConnection(_connectrionString))
            {
                await conn.OpenAsync();
                using (var cmdInsert = new SqlCommand())
                {
                    cmdInsert.Connection = conn;
                    cmdInsert.CommandText = @"
                        INSERT INTO CONNECTPARTS.DBO.PRECIFICACOES(
			                        PRODUTOCODIGOEXTERNO, 
			                        DATAATUALIZACAOREGISTRO, 
			                        DATACRIACAOREGISTRO,
			                        DATACONFIRMACAO,
			                        APROVADORRESPONSAVELEMAIL, 
			                        APROVADO, 
			                        PRODUTOLISTAPRECOCODIGO,
			                        PRODUTOPRECOTABELANOVO, 
			                        PRODUTOPRECOPROMOCIONALNOVO)  
                        VALUES (
			                        @PRODUTOCODIGOEXTERNO,
			                        @DATAATUALIZACAOREGISTRO,
			                        @DATACRIACAOREGISTRO,
			                        @DATACONFIRMACAO,
			                        @APROVADORRESPONSAVELEMAIL,
			                        @APROVADO,
			                        @PRODUTOLISTAPRECOCODIGO,
			                        @PRODUTOPRECOTABELANOVO,
			                        @PRODUTOPRECOPROMOCIONALNOVO)";

                    foreach (var registro in registros)
                    {
                        cmdInsert.Parameters.AddWithValue("@PRODUTOCODIGOEXTERNO", registro.PRODUTOCODIGOEXTERNO);
                        cmdInsert.Parameters.AddWithValue("@DATAATUALIZACAOREGISTRO", registro.DATAATUALIZACAOREGISTRO);
                        cmdInsert.Parameters.AddWithValue("@DATACRIACAOREGISTRO", registro.DATACRIACAOREGISTRO);
                        cmdInsert.Parameters.AddWithValue("@DATACONFIRMACAO", registro.DATACONFIRMACAO);
                        cmdInsert.Parameters.AddWithValue("@APROVADORRESPONSAVELEMAIL", registro.APROVADORRESPONSAVELEMAIL);
                        cmdInsert.Parameters.AddWithValue("@APROVADO", registro.APROVADO);
                        cmdInsert.Parameters.AddWithValue("@PRODUTOLISTAPRECOCODIGO", registro.PRODUTOLISTAPRECOCODIGO);
                        cmdInsert.Parameters.AddWithValue("@PRODUTOPRECOTABELANOVO", registro.PRODUTOPRECOTABELANOVO);
                        cmdInsert.Parameters.AddWithValue("@PRODUTOPRECOPROMOCIONALNOVO", registro.PRODUTOPRECOPROMOCIONALNOVO);

                        await cmdInsert.ExecuteNonQueryAsync();

                        // Limpar os parâmetros
                        cmdInsert.Parameters.Clear();
                    }
                }
            }
            return registros;
        }
    }
}

