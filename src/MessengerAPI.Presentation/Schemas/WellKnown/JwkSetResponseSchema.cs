using MessengerAPI.Application.Schemas.Common;

namespace MessengerAPI.Presentation.Schemas.WellKnown;

public record JwkSetResponseSchema(List<JsonWebKeySchema> Keys);
