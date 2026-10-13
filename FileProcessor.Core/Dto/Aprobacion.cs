namespace FileProcessor.Core.Dto
{
    public class Aprobacion
    {
        public int IDAPROBACIONPOLIZA { get; set; }
        public long IDPOLIZA { get; set; }
        public int IDUSUARIO { get; set; }
        public int IDCATESTATUS { get; set; }
        public string NOMBREARCHIVO { get; set; } = string.Empty;
    }
}
