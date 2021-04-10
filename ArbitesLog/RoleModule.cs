using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System.Linq;
using Discord.WebSocket;

namespace ArbitesLog
{
	public class RoleModule : ModuleBase<SocketCommandContext>
	{

		//[RequireUserPermission(GuildPermission.Administrator)]
		//[Command("SetupRole")]
		//[Summary("Makes a Role Available or Unavailable for users to add")]
		//public async Task SetupRoleAsync(SocketRole role)
		//{

		//	GuildData data = GuildManager.GetGuildData(Context.Guild.Id).Result;
		//	List<ulong> roles = data.AssignableRoles;


		//	data.AssignableRoles = roles.ToList();
		//	await GuildManager.SetGuildData(data);
		//}

		[RequireUserPermission(GuildPermission.Administrator)]
		[Command("RoleSet")]
		[Summary("Make Role User Settable")]
		public async Task RoleSetAsync(params SocketRole[] roles)
		{
			ulong name = Context.Guild.Id;
			GuildData data;

			if (!GuildManager.CheckGuildData(name))
			{
				await ReplyAsync("No GuildData for this Guild!");
				return;
			}
			else
			{
				data = GuildManager.GetGuildData(name).Result;
			}

			foreach (SocketRole role in roles)
			{
				data.AssignableRoles.Add(role.Id);
				await ReplyAsync($"Set Role {role.Name} to self-apply!");
			}
			await GuildManager.SetGuildData(data);

		}
		[RequireUserPermission(GuildPermission.Administrator)]
		[Command("RoleUnset")]
		[Summary("Make Role User Settable")]
		public async Task RoleUnSetAsync(params SocketRole[] roles)
		{
			ulong name = Context.Guild.Id;
			GuildData data;

			if (!GuildManager.CheckGuildData(name))
			{
				await ReplyAsync("No GuildData for this Guild!");
				return;
			}
			else
			{
				data = GuildManager.GetGuildData(name).Result;
			}

			foreach (SocketRole role in roles)
			{
				if (data.AssignableRoles.Contains(role.Id))
				{
					data.AssignableRoles.Remove(role.Id);
					await ReplyAsync($"Unset Role {role.Name} from self-apply!");
				}
			}
			await GuildManager.SetGuildData(data);
		}


		//[Command("Role")]
		//[Summary("Adds a role to the user who requests it")]
		//public async Task RoleAsync(string role)
		//{
		//	ICollection<IRole> roles = GuildManager.GetGuildData(Context.Guild.Id).Result.AssignableRoles;
		//	ICollection<IRole> userRoles = (Context.User as SocketGuildUser).Roles as ICollection<IRole>;
		//	var result = from r in roles where r.Name == role select r;
		//	if (result == null)
		//	{
		//		await ReplyAsync($"Role {result.First().Name} Cannot be Customised.");
		//	}
		//	else
		//	{
		//		var userResult = from r in userRoles where r.Name == role select r;
		//		if (userResult == null)
		//		{
		//			await (Context.User as SocketGuildUser).AddRoleAsync(userResult.First());
		//		}
		//		else
		//		{
		//			await (Context.User as SocketGuildUser).RemoveRoleAsync(userResult.First());

		//		}
		//	}
		//}

		[Command("Role", RunMode = RunMode.Async)]
		[Summary("Adds or Removes a Setable Role")]
		public async Task RoleAsync(SocketRole role)
		{
			ulong servID = Context.Guild.Id;
			List<ulong> userRoles = new List<ulong>();

			foreach (SocketRole usrRole in Context.Guild.GetUser(Context.User.Id).Roles)
			{
				userRoles.Add(usrRole.Id);
			}


			if (GuildManager.CheckGuildData(servID))
			{
				GuildData data;
				data = GuildManager.GetGuildData(servID).Result;
				foreach (ulong roleID in data.AssignableRoles)
				{
					if (roleID == role.Id)
					{

						if (userRoles.Contains(role.Id))
						{
							await Context.Guild.GetUser(Context.User.Id).RemoveRoleAsync(role);
							await ReplyAsync($"Removed {role.Name}!");

						}
						else
						{
							await Context.Guild.GetUser(Context.User.Id).AddRoleAsync(role);
							await ReplyAsync($"Added {role.Name}!");
						}
					}
				}
			}
			else
			{
				await ReplyAsync("No GuildData for this Guild!");
			}
		}

	}
}
