using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
using AutomatedTaskSystem.Services.ResponseService;
using Microsoft.AspNetCore.Mvc;

namespace AutomatedTaskSystem.Controllers
{
    [Route("nodes")]
    [ApiController]
    public class NodeController : ControllerBase
    {
        private readonly DataContext _context;

        public NodeController(DataContext context) => _context = context;

        private async Task<ActionResult<Responses.NodeDTO>> getNode(int nodeId)
        {
            var node = await _context.Nodes
                .Where(_n => _n.Id == nodeId && !_n.Archived)
                .Include(_n => _n.Previous)
                .Include(_n => _n.Next)
                .Include(_n => _n.Required)
                .Include(_n => _n.Requires)
                .FirstOrDefaultAsync();

            if (node == null)
            {
                return NotFound(new Responses.BadRequestsDTO("Node not found"));
            }

            var res = new Responses.NodeDTO
            {
                Id = node.Id,
                Order = node.Order,
                isEnd = node.isEnd,
                isStart = node.isStart,
                Name = node.Name,
            };

            node.Previous.ForEach(
                _n => res.Previous.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
            );
            node.Next.ForEach(
                _n => res.Next.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
            );
            node.Required.ForEach(
                _n => res.Required.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
            );
            node.Requires.ForEach(
                _n => res.Requires.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
            );
            var steps = await _context.Steps
                .Where(_s => _s.NodeId == node.Id)
                .Include(_s => _s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .ToListAsync();

            steps.ForEach(_s =>
            {
                var nodeStep = new Responses.NodeStepDTO
                {
                    Id = _s.Id,
                    Name = _s.TaskBank.Name,
                    Order = _s.Order,
                    Reviewable = _s.TaskBank.TypeId == 3,
                    TL = _s.TaskBank.TL,
                    Group = new Responses.IDName
                    {
                        Name = _s.TaskBank.Group.Name,
                        Id = _s.TaskBank.GroupId,
                    },
                    Duration = _s.Duration,
                    Priority = _s.Priority
                };

                res.Steps.Add(nodeStep);
            });

            res.Steps.Sort(
                (a, b) =>
                {
                    if (a.Order > b.Order)
                        return 1;
                    if (a.Order < b.Order)
                        return -1;
                    return 0;
                }
            );

            return res;
        }

        // PATCH:
        // Edit Existing Node
        [HttpPatch("{nodeId}")]
        public async Task<ActionResult<Responses.NodeDTO>> EditNode(
            int nodeId,
            Requests.NodeDTO req
        )
        {
            var node = await _context.Nodes
                .Where(n => n.Id == nodeId && !n.Archived)
                .Include(n => n.Previous)
                .Include(n => n.Next)
                .Include(n => n.Steps)
                .ThenInclude(s => s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .FirstOrDefaultAsync();

            if (node == null)
            {
                return NotFound(new Responses.BadRequestsDTO("Node not found"));
            }

            var prevNodes = await _context.Nodes
                .Where(n => req.Previous.Contains(n.Id) && !n.Archived)
                .Include(n => n.Previous)
                .Include(n => n.Next)
                .Include(n => n.Requires)
                .Include(n => n.Required)
                .ToListAsync();

            var requires = await _context.Nodes
                .Where(n => req.Requires.Contains(n.Id) && !n.Archived)
                .Include(n => n.Previous)
                .Include(n => n.Next)
                .Include(n => n.Requires)
                .Include(n => n.Required)
                .ToListAsync();

            foreach (var item in requires)
                item.Required.Add(node);

            foreach (var item in prevNodes)
                item.Next.Add(node);

            node.Requires = requires;
            node.Previous = prevNodes;

            if (req.Name != "")
                node.Name = req.Name;
            node.isStart = req.IsStart;

            await _context.SaveChangesAsync();

            var steps = new List<Responses.NodeStepDTO> { };
            foreach (var item in node.Steps)
            {
                var groups = new List<Responses.IDName> { };
                steps.Add(
                    new Responses.NodeStepDTO
                    {
                        Priority = item.Priority,
                        Id = item.Id,
                        TL = item.TaskBank.TL,
                        Name = item.TaskBank.Name,
                        Order = item.Order,
                        Group = new Responses.IDName
                        {
                            Name = item.TaskBank.Name,
                            Id = item.TaskBank.Id
                        },
                        Reviewable = item.TaskBank.TypeId == 3,
                        Duration = item.Duration
                    }
                );
            }

            var prevs = new List<Responses.IDName> { };
            foreach (var item in node.Previous)
                prevs.Add(new Responses.IDName { Name = item.Name, Id = item.Id });

            var nexts = new List<Responses.IDName> { };
            foreach (var item in node.Next)
                nexts.Add(new Responses.IDName { Name = item.Name, Id = item.Id });

            var reqs = new List<Responses.IDName> { };
            foreach (var item in node.Requires)
                reqs.Add(new Responses.IDName { Name = item.Name, Id = item.Id });

            var reqd = new List<Responses.IDName> { };
            foreach (var item in node.Required)
                reqs.Add(new Responses.IDName { Name = item.Name, Id = item.Id });

            var res = new Responses.NodeDTO
            {
                Order = node.Order,
                Id = node.Id,
                Name = node.Name,
                isEnd = node.isEnd,
                isStart = node.isStart,
                Next = nexts,
                Previous = prevs,
                Steps = steps,
                Required = reqd,
                Requires = reqs
            };

            return res;
        }

        // POST:
        // Create new Node
        [HttpPost("{schemaId}")]
        public async Task<ActionResult<Responses.NodeDTO>> CreateNode(
            int schemaId,
            Requests.NodeDTO req
        )
        {
            var schema = await _context.Schemas
                .Where(_s => _s.Id == schemaId)
                .FirstOrDefaultAsync();

            if (schema == null)
            {
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));
            }

            var previousNodes = new List<Node> { };
            var requiresNodes = new List<Node> { };
            var emptyNodes = new List<Node> { };

            var newNode = new Node
            {
                isEnd = true,
                isStart = req.IsStart,
                Name = req.Name,
                Schema = schema,
                SchemaId = schema.Id,
                Previous = previousNodes,
                Next = emptyNodes,
                Required = emptyNodes,
                Requires = requiresNodes,
                Steps = new List<Step> { }
            };

            for (int i = 0; i < req.Previous.Count; i++)
            {
                var _pn = await _context.Nodes
                    .Where(_n => _n.Id == req.Previous[i] && !_n.Archived)
                    .Include(_n => _n.Next)
                    .FirstOrDefaultAsync();

                if (_pn == null)
                {
                    return NotFound(
                        new Responses.BadRequestsDTO($"Node of ID {req.Previous[i]} is not found")
                    );
                }
                _pn.isEnd = false;
                _pn.Next.Add(newNode);

                previousNodes.Add(_pn);
            }
            for (int i = 0; i < req.Requires.Count; i++)
            {
                var _pn = await _context.Nodes
                    .Where(_n => _n.Id == req.Requires[i] && !_n.Archived)
                    .Include(_n => _n.Next)
                    .FirstOrDefaultAsync();

                if (_pn == null)
                {
                    return NotFound(
                        new Responses.BadRequestsDTO($"Node of ID {req.Requires[i]} is not found")
                    );
                }

                _pn.Required.Add(newNode);

                requiresNodes.Add(_pn);
            }
            _context.Nodes.Add(newNode);
            await _context.SaveChangesAsync();

            return await getNode(newNode.Id);
        }

        // GET:
        // Fetch all schema's Nodes
        [HttpGet("{schemaId}/mini")]
        public async Task<ActionResult<List<Responses.IDName>>> GetNodesMini(int schemaId)
        {
            var schema = await _context.Schemas
                .Where(_s => _s.Id == schemaId)
                .Include(_ => _.Nodes)
                .FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var res = new List<Responses.IDName> { };

            foreach (var node in schema.Nodes)
                res.Add(new Responses.IDName { Name = node.Name, Id = node.Id });

            return res;
        }

        // GET:
        // Fetch all schema's Nodes
        [HttpGet("{schemaId}")]
        public async Task<ActionResult<List<Responses.NodeDTO>>> GetNodes(int schemaId)
        {
            var schema = await _context.Schemas
                .Where(_s => _s.Id == schemaId)
                .FirstOrDefaultAsync();

            if (schema == null)
                return NotFound(new Responses.BadRequestsDTO("Schema not found"));

            var nodes = await _context.Nodes
                .Where(_n => _n.SchemaId == schemaId && !_n.Archived)
                .Include(_n => _n.Previous)
                .Include(_n => _n.Next)
                .Include(_n => _n.Required)
                .Include(_n => _n.Requires)
                .Include(n => n.Steps)
                .ThenInclude(s => s.TaskBank)
                .ThenInclude(tb => tb.Group)
                .ToListAsync();

            var res = new List<Responses.NodeDTO> { };

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                if (node == null)
                    return BadRequest("An error poped up");

                var nodeRes = new Responses.NodeDTO
                {
                    Id = node.Id,
                    isEnd = node.isEnd,
                    isStart = node.isStart,
                    Name = node.Name,
                    Order = node.Order
                };

                node.Previous.ForEach(
                    _n => nodeRes.Previous.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
                );
                node.Next.ForEach(
                    _n => nodeRes.Next.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
                );
                node.Required.ForEach(
                    _n => nodeRes.Required.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
                );
                node.Requires.ForEach(
                    _n => nodeRes.Requires.Add(new Responses.IDName { Id = _n.Id, Name = _n.Name })
                );

                node.Steps.ForEach(_s =>
                {
                    if (_s.Archived)
                        return;
                    var nodeStep = new Responses.NodeStepDTO
                    {
                        Priority = _s.Priority,
                        Id = _s.Id,
                        Name = _s.TaskBank.Name,
                        Order = _s.Order,
                        Reviewable = _s.TaskBank.TypeId == 3,
                        TL = _s.TaskBank.TL,
                        Group = new Responses.IDName
                        {
                            Name = _s.TaskBank.Group.Name,
                            Id = _s.TaskBank.Group.Id
                        },
                        Duration = _s.Duration
                    };

                    nodeRes.Steps.Add(nodeStep);
                });
                nodeRes.Steps.Sort(
                    (a, b) =>
                    {
                        if (a.Order > b.Order)
                            return 1;
                        if (a.Order < b.Order)
                            return -1;
                        return 0;
                    }
                );
                res.Add(nodeRes);
            }

            return res;
        }

        // DELETE:
        // Delete node
        [HttpDelete("{id}")]
        public async Task<ActionResult<Responses.SuccessDTO>> DeleteNode(int id)
        {
            var node = await _context.Nodes
                .Where(n => n.Id == id && !n.Archived)
                .Include(n => n.Next)
                .ThenInclude(n => n.Previous)
                .Include(n => n.Previous)
                .ThenInclude(n => n.Next)
                .Include(n => n.Required)
                .ThenInclude(n => n.Requires)
                .Include(n => n.Requires)
                .ThenInclude(n => n.Required)
                .Include(n => n.Steps)
                .FirstOrDefaultAsync();

            if (node == null)
                return NotFound(new Responses.BadRequestsDTO("Node not found"));

            foreach (var item in node.Next)
            {
                if (node.isStart && item.Previous.Count <= 1)
                    item.isStart = true;
                else
                    foreach (var _ in node.Previous)
                    {
                        item.Previous.Add(_);
                        _.Next.Add(item);
                    }
            }

            foreach (var item in node.Next)
                item.Previous.Remove(node);
            foreach (var item in node.Previous)
                item.Next.Remove(node);

            foreach (var item in node.Required)
                item.Requires.Remove(node);
            foreach (var item in node.Requires)
                item.Required.Remove(node);

            foreach (var step in node.Steps)
                step.Archived = true;

            node.Archived = true;
            await _context.SaveChangesAsync();

            return Ok(new Responses.SuccessDTO("Node Delete"));
        }

        [HttpPatch("{nodeId}/up")]
        public async Task<ActionResult<List<Responses.NodeDTO>>> OrderNodeUp(int nodeId)
        {
            var node = await _context.Nodes.Where(n => n.Id == nodeId).FirstOrDefaultAsync();

            if (node is null)
                return BadRequest(
                    new BaseResponseService { Error = false, Message = "Node is not found" }
                );

            var nodes = await _context.Nodes
                .Where(n => n.SchemaId == node.SchemaId && !n.Archived)
                .Include(n => n.Previous)
                .Include(n => n.Next)
                .Include(n => n.Required)
                .Include(n => n.Requires)
                .ToListAsync();

            if (node.Order > 1)
            {
                var prevNode = nodes.Where(n => n.Order + 1 == node.Order).FirstOrDefault();

                if (prevNode is not null)
                    prevNode.Order++;
                node.Order--;
            }

            await _context.SaveChangesAsync();

            return await GetNodes(node.SchemaId);
        }

        [HttpPatch("{nodeId}/down")]
        public async Task<ActionResult<List<Responses.NodeDTO>>> OrderNodeDown(int nodeId)
        {
            var node = await _context.Nodes.Where(n => n.Id == nodeId).FirstOrDefaultAsync();

            if (node is null)
                return BadRequest(
                    new BaseResponseService { Error = false, Message = "Node is not found" }
                );

            var nodes = await _context.Nodes
                .Where(n => n.SchemaId == node.SchemaId && !n.Archived)
                .Include(n => n.Previous)
                .Include(n => n.Next)
                .Include(n => n.Required)
                .Include(n => n.Requires)
                .ToListAsync();

            if (nodes.Any(n => n.Order > node.Order))
            {
                var prevNode = nodes.Where(n => n.Order - 1 == node.Order).FirstOrDefault();

                if (prevNode is not null)
                    prevNode.Order--;
                node.Order++;
            }
            await _context.SaveChangesAsync();

            return await GetNodes(node.SchemaId);
        }

        [HttpGet("/OTR")]
        public async Task<ActionResult<BaseResponseService>> DeleteNode()
        {
            var schemas = await _context.Schemas
                .Where(s => !s.Archived)
                .Include(s => s.Nodes)
                .ToListAsync();

            foreach (var schema in schemas)
            {
                int count = 1;
                foreach (var node in schema.Nodes)
                    node.Order = count++;
            }

            await _context.SaveChangesAsync();

            return Ok(new BaseResponseService { Error = false, Message = "Nodes ordered" });
        }

        [HttpPatch("/OTR")]
        public async Task<ActionResult<BaseResponseService>> OTR()
        {
            var los = await _context.LearningObjectives
                .Where(p => !p.Archived)
                .Include(lo => lo.Tasks)
                .ToListAsync();

            var taskActivities = await _context.Activities.ToListAsync();

            foreach (var lo in los)
            {
                var done = true;

                DateTime latest = new DateTime();

                foreach (var task in lo.Tasks)
                {
                    if (task.StatusId != 4 && !task.Archived)
                        done = false;

                    var acts = taskActivities
                        .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 1)
                        .ToList();
                    foreach (var item in acts)
                    {
                        if (lo.StartedAt is null || item.TimeStamp < lo.StartedAt)
                            lo.StartedAt = item.TimeStamp;
                    }

                    if (done)
                    {
                        acts = taskActivities
                            .Where(a => a.TaskId == task.Id && a.ActivityTypeId == 2)
                            .ToList();
                        foreach (var item in acts)
                        {
                            if (lo.StartedAt is null || item.TimeStamp < lo.StartedAt)
                                latest = item.TimeStamp;
                        }
                    }
                }

                if (done)
                    lo.DoneAt = latest;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
