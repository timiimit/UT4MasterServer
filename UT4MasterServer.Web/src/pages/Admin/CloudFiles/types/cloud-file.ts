export interface ICloudFile {
  accountID: string;
  filename: string;
  hash: string;
  hash256: string;
  uploadedAt: string;
  rawContent: string;
  length: number;
}
