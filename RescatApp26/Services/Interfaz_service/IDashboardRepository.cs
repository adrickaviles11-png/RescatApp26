using RescatApp26.Models.DTOs;

namespace RescatApp26.Services.Interfaz_service;

public interface IDashboardRepository
{
    DashboardDto GetSummary(DateTime today);
}