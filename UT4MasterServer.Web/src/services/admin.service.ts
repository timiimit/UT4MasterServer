import { Role } from '@/enums/role';
import { IAdminChangePasswordRequest } from '@/pages/Admin/Accounts/types/admin-change-password-request';
import { IClient } from '@/pages/Admin/Clients/types/client';
import { ICloudFile } from '@/pages/Admin/CloudFiles/types/cloud-file';
import { ITrustedGameServer } from '@/pages/Admin/TrustedServers/types/trusted-game-server';
import HttpService from './http.service';

export default class AdminService extends HttpService {
  private baseUrl = `${__BACKEND_URL}/admin`;

  // Account
  async getRoleOptions() {
    return await this.get<Role[]>(`${this.baseUrl}/flags`);
  }

  async getRolesForAccount(id: string) {
    return await this.get<Role[]>(`${this.baseUrl}/flags/${id}`);
  }

  async setRolesForAccount(id: string, roles: Role[]) {
    return await this.put<string[]>(
      `${this.baseUrl}/flags/${id}`,
      { body: roles },
      false
    );
  }

  async deleteAccount(id: string) {
    return await this.delete(
      `${this.baseUrl}/account/${id}`,
      { body: true },
      false
    );
  }

  async changePassword(id: string, request: IAdminChangePasswordRequest) {
    return await this.patch(
      `${this.baseUrl}/change_password/${id}`,
      { body: request },
      false
    );
  }

  // Trusted Servers
  async getTrustedServers() {
    return await this.get<ITrustedGameServer[]>(
      `${this.baseUrl}/trusted_servers`
    );
  }

  async createTrustedServer(request: Partial<ITrustedGameServer>) {
    return await this.post(
      `${this.baseUrl}/trusted_servers`,
      { body: request },
      false
    );
  }

  async updateTrustedServer(id: string, request: ITrustedGameServer) {
    return await this.patch(
      `${this.baseUrl}/trusted_servers/${id}`,
      { body: request },
      false
    );
  }

  async deleteTrustedServer(id: string) {
    return await this.delete(`${this.baseUrl}/trusted_servers/${id}`);
  }

  // Clients
  async getClients() {
    return await this.get<IClient[]>(`${this.baseUrl}/clients`);
  }

  async createClient(name: string) {
    return await this.post(
      `${this.baseUrl}/clients/new`,
      { body: name },
      false
    );
  }

  async updateClient(id: string, request: IClient) {
    return await this.patch(
      `${this.baseUrl}/clients/${id}`,
      { body: request },
      false
    );
  }

  async deleteClient(id: string) {
    return await this.delete(`${this.baseUrl}/clients/${id}`);
  }

  // Cloud Files
  async getCloudFiles() {
    return await this.get<ICloudFile[]>(`${this.baseUrl}/mcp_files`);
  }

  async upsertCloudFile(formData: FormData) {
    return await this.post(`${this.baseUrl}/mcp_files`, { formData });
  }

  async deleteCloudFile(name: string) {
    return await this.delete(`${this.baseUrl}/mcp_files/${name}`);
  }
}
