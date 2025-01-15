public record ProductCode
{
    public string NcpConsumption { get; set; } = "PDC-NCP";
    public string NcpPcp { get; set; } = "PDC-NPD";
    public string NcpWorks { get; set; } = "PDC-NWD";
    public string NcpManagedService { get; set; } = "PDC-NMS";
    public string Cutoff100 { get; set; } = "PDC-004";
}