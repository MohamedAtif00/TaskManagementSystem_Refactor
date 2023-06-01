using AutomatedTaskSystem.Data;
using AutomatedTaskSystem.DTO;
using AutomatedTaskSystem.Models;
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
                .ToListAsync();

            var res = new List<Responses.NodeDTO> { };

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                // await _context.Nodes
                //     .Where(_n => _n.Id == nodes[i].Id)
                //     .Include(_n => _n.Previous)
                //     .Include(_n => _n.Next)
                //     .Include(_n => _n.Required)
                //     .Include(_n => _n.Requires)
                //     .FirstOrDefaultAsync();

                if (node == null)
                    return BadRequest("An error poped up");

                var nodeRes = new Responses.NodeDTO
                {
                    Id = node.Id,
                    isEnd = node.isEnd,
                    isStart = node.isStart,
                    Name = node.Name,
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
                var steps = await _context.Steps
                    .Where(_s => _s.NodeId == node.Id && !_s.Archived)
                    .Include(_s => _s.TaskBank)
                    .ThenInclude(tb => tb.Group)
                    .ToListAsync();

                steps.ForEach(_s =>
                {
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
    }
}
