import { Statistic } from "../enums/statistic";

export interface IStatisticCard {
    heading: string;
    stats: Statistic[];
    headingIcon?: string;
}

export interface IStatisticSection {
    heading: string;
    cards: IStatisticCard[];
}