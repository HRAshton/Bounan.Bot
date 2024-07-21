﻿import { validateMyAnimeListId } from './argument-validators';
import { CommandDto } from './command-dto';
import { CommandShortcut } from './command-shortcut';

export class DubsCommandDto extends CommandDto {
    public static Command = CommandShortcut.dub;

    public command = DubsCommandDto.Command;
    protected properties: (keyof this)[] = ['myAnimeListId'];

    public myAnimeListId: number;

    constructor(myAnimeListId: number | string) {
        super();
        this.myAnimeListId = typeof myAnimeListId === 'string' ? parseInt(myAnimeListId) : myAnimeListId;
        validateMyAnimeListId(this.myAnimeListId);
    }
}