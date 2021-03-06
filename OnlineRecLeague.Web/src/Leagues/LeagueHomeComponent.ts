import * as ko from "knockout";
import * as UrlRouterComponent from "UrlRouter/UrlRouterComponent";
import { JsonRequestProxy, IJsonRequestProxy } from "JsonRequestProxy";

export const RoutedComponent: UrlRouterComponent.RoutedComponent = {
	ComponentName: "LadderHome",
	CreateComponentParams: (matches) => ({ Path: matches[0] }),
	MatchRegex: /^\/l\/([^\/]+)$/i,
};

interface LeagueResponse {
	Name: string;
	Path: string;
}

interface LeagueParams {
	Path: string;
}

class LeagueHome {
	constructor(params: LeagueParams, jsonRequest?: IJsonRequestProxy) {
		this.JsonRequest = jsonRequest || new JsonRequestProxy();

		this.Path = params.Path;
		this.IsLoading = ko.observable(false);
		this.LoadError = ko.observable("");
		this.Ladder = ko.observable();

		this.TryLoadLadder();
	}

	public Path: string;
	public IsLoading: ko.Observable<boolean>;
	public LoadError: ko.Observable<string>;
	public Ladder: ko.Observable<LeagueResponse|null>;

	private TryLoadLadder() {
		this.JsonRequest
			.Get<LeagueResponse>(`/api/leagues/find?path=${this.Path}`, this.IsLoading)
			.OnSuccess((response) => this.Ladder(response))
			.OnFailure((error) => this.LoadError(error));
	}

	private JsonRequest: IJsonRequestProxy;
}

ko.components.register(RoutedComponent.ComponentName, {
	viewModel: LeagueHome,
	template: `
	<div>
		<div>Ladder Home</div>
		<div data-bind="text: $component.Path" />
		<div data-bind="visible: $component.IsLoading">IsLoading</div>
		<div data-bind="text: $component.LoadError"></div>
	</div>`,
});
