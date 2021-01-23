/* eslint-disable jsx-a11y/anchor-is-valid */
import React from "react";
import { QueryClient, QueryClientProvider, useQuery } from "react-query";
import { Grid, Image, Icon } from 'semantic-ui-react'
import { Statistic } from 'semantic-ui-react'
const queryClient = new QueryClient();

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <Example />
    </QueryClientProvider>
  );
}

function Example() {
  const { isLoading, error, data, isFetching } = useQuery("repoData", () =>
    fetch(
      "/ping"
    ).then((res) => res.json()), {
      refetchInterval: 1000
    }
  );

  if (isLoading) return "Loading...";

  if (error) return "An error has occurred: " + error.message;

  const wattKg = data.power / 73.93;

  return (
    <Grid columns={2} divided inverted>
      <Grid.Row>
        <Grid.Column width={1} textAlign="center" verticalAlign="middle">
        <Image
            size='small'
            src='stryd-logo.png'
            centered
            verticalAlign='middle'
          />
        </Grid.Column>
        <Grid.Column>
            <Statistic.Group>
          <Statistic>
            <Statistic.Value>
              {data.power}
            </Statistic.Value>
            <Statistic.Label>
              watts
            </Statistic.Label>
          </Statistic>
          <Statistic>
            <Statistic.Value>
            {wattKg.toFixed(2)}
            </Statistic.Value>
            <Statistic.Label>
              w/kg
            </Statistic.Label>
          </Statistic>
          
        </Statistic.Group>
        </Grid.Column>
      </Grid.Row>
    </Grid>
    
    
  );
}