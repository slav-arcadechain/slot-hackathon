import { CheckCircleIcon, SettingsIcon } from '@chakra-ui/icons';
import { Heading, VStack, List, ListIcon, ListItem } from '@chakra-ui/react';
// var __html = require('../../../../public/slot/index.html');
// var template = { __html: __html };

const Home = () => {
  return (
    <VStack w={'full'}>

      {/*<div dangerouslySetInnerHTML={template} />*/}

        <iframe src="/slot/index.html" width={960} height={642}/>


    </VStack>
  );
};

export default Home;
