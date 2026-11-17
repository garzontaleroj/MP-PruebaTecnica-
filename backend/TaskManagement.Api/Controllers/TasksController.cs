using Microsoft.AspNetCore.Mvc;
using TaskManagement.Api.DTOs;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TaskResponse>> Create(
        CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] string? status,
        CancellationToken cancellationToken) =>
        Ok(await taskService.GetAllAsync(userId, status, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskResponse>> GetById(Guid id, CancellationToken cancellationToken) =>
        Ok(await taskService.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TaskResponse>> UpdateStatus(
        Guid id,
        UpdateTaskStatusRequest request,
        CancellationToken cancellationToken) =>
        Ok(await taskService.UpdateStatusAsync(id, request.Status, cancellationToken));
}
