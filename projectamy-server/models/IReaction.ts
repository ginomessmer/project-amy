import { NullableOption } from "@microsoft/microsoft-graph-types";

export interface IReaction {
    reactionType: string;
    name: NullableOption<string>;
}