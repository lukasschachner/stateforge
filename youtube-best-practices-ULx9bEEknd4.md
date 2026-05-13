# Best Practices Extracted from YouTube Video ULx9bEEknd4

Source: <https://www.youtube.com/watch?v=ULx9bEEknd4>

> Note: These are the practices and recommendations expressed in the video transcript. The speaker presents them as workflow-dependent preferences, not absolute rules for every team or codebase.

## 1. Prefer vertical slice architecture for many application codebases

- Organize code by feature or business capability rather than by technical layer.
- Put the files needed for one feature close together, such as endpoints, services, repositories, DTOs, mapping code, and feature-specific logic.
- Use folders such as `Auth`, `Users`, `Basket`, or similar domain features so the purpose of each area is immediately visible.
- Favor this style when it reduces project hopping and makes changes easier to localize.
- For monoliths, vertical slices can make future service extraction easier because a feature is already grouped in one place.
- Vertical slices may also work better with AI coding agents because the relevant context is concentrated in fewer locations.

## 2. Avoid unnecessary layering and abstraction

- Do not add layers just because an architecture template includes them.
- Prefer direct, understandable call chains over many indirections.
- Keep code easy to follow for humans, debuggers, and AI coding tools.
- If a feature can be implemented clearly with fewer layers, choose the simpler design.

## 3. Use a testing diamond instead of relying primarily on unit tests

- Write many integration tests that exercise the system through realistic boundaries.
- Keep a small number of end-to-end tests for full workflow coverage.
- Use unit tests mainly for edge cases that are hard to reach through normal integration flows.
- In .NET, use `WebApplicationFactory` to run APIs/services in-process for integration testing.
- Use Testcontainers when tests need real infrastructure such as databases or queues.
- Design integration test setup carefully so the tests remain fast and maintainable.
- Prefer tests that describe how users or clients interact with the system rather than tests that mirror internal method-by-method implementation details.

## 4. Keep mapping explicit

- Preserve the separation between API contracts, domain models, and persistence models.
- Avoid convention-based automatic mappers when explicit mapping is safer and easier to reason about.
- Prefer simple mapping methods, such as extension methods:
  - `entity.ToResponse()`
  - `request.ToDomain()`
  - `dto.ToDomain()`
- Make transformations visible in code so reviewers and tools can verify exactly what is exposed or persisted.
- Do not choose mapping libraries only for convenience if they hide important conversion behavior.
- Source-generated mappers may reduce performance overhead, but they do not remove the concern that mapping behavior can become less explicit.

## 5. Avoid mediator libraries when direct service calls are enough

- Do not insert a mediator layer unless it provides clear value for the specific application.
- A controller or minimal API endpoint can call an application service directly.
- Avoid chains like endpoint/controller → mediator → handler → service → repository when they add no meaningful separation.
- Prefer designs that are easier to step through in a debugger.
- Reduce indirection to make code cheaper and easier for AI tools to analyze.

## 6. Be pragmatic with repositories, especially with EF Core

- Do not add a repository abstraction over Entity Framework Core by default.
- When EF Core is already your data access abstraction, using `DbContext` directly can be simpler and clearer.
- Add repositories only when they solve a real problem in the codebase, not as a default pattern.

## 7. Treat EF Core as a valid default choice

- Do not reject EF Core purely because Dapper can be faster in some scenarios.
- EF Core is performant enough for many applications and has many useful features.
- In many real systems, the network/database round trip is more significant than EF Core overhead.
- Use EF Core for reads and writes if it fits the team and application.
- A hybrid approach is also reasonable, such as:
  - EF Core for writes and migrations
  - Dapper for reads
- Dapper remains a good option when explicit SQL control is preferred.
- Choose the data access approach based on workflow, maintainability, and actual performance needs.

## 8. Optimize for clarity in the AI-assisted development era

- Prefer code organization that makes relevant context easy to find.
- Reduce scattered files, unnecessary layers, and convention-heavy behavior.
- Make important behavior explicit so AI agents are less likely to infer incorrectly.
- Simpler structure can reduce development latency and mistakes when using tools such as coding agents.

## 9. Reevaluate practices as experience and workflow change

- Practices that were useful earlier may stop being the best fit later.
- Keep architectural habits open to revision as tooling, team experience, and project needs change.
- Avoid treating any pattern as universally correct.
- Choose practices that improve productivity, confidence, and maintainability in the current context.
